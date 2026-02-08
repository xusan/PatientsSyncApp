using Core.Contracts.Services;
using Core.Models;
using EfDataStorage;
using Microsoft.Extensions.DependencyInjection;
using UnitTests.AppServices;

namespace IntegrationTest.AppServices
{
    [TestCategory("Integration")]
    [TestClass]
    public class PatientTest : BaseAppServiceTest
    {
        private IPatientsService _service = null!;

        [TestInitialize]
        public void Init()
        {
            _service = _provider.GetRequiredService<IPatientsService>();
        }

        [TestMethod]
        ///returns empty when DB empty
        public async Task GetAllAsync_ReturnsEmpty_WhenNoPatients()
        {
            var result = await _service.GetAllAsync();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Result.Count);
        }

        [TestMethod]
        //returns all patients
        public async Task GetAllAsync_ReturnsAllPatients()
        {
            using var scope = _provider.CreateScope();
            var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var maxId = GetMaxId();
            _db.Patients.AddRange(
                new PatientModel { Id = maxId + 1, Name = "John" },
                new PatientModel { Id = maxId + 2, Name = "Kate" });

            await _db.SaveChangesAsync();

            var result = await _service.GetAllAsync();

            Assert.AreEqual(2, result.Result.Count);
        }


        [TestMethod]
        //paging returns correct subset
        public async Task GetPatientsPageAsync_ReturnsCorrectPage()
        {
            using var scope = _provider.CreateScope();
            var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var maxId = GetMaxId();
            for (int i = 0; i < 10; i++)
            {
                _db.Patients.Add(new PatientModel { Id = ++maxId, Name = $"P{i}" });
            }

            await _db.SaveChangesAsync();

            var result = await _service.GetPatientsPageAsync(5, 3);

            Assert.AreEqual(3, result.Result.Count);
            Assert.AreEqual("P5", result.Result[0].Name);
        }




        [TestMethod]
        //skip beyond count returns empty
        public async Task GetPatientsPageAsync_ReturnsEmpty_WhenSkipTooLarge()
        {
            using var scope = _provider.CreateScope();
            var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var maxId = GetMaxId();
            _db.Patients.Add(new PatientModel { Id = maxId + 1, Name = "Only" });
            await _db.SaveChangesAsync();

            var result = await _service.GetPatientsPageAsync(10, 5);

            Assert.AreEqual(0, result.Result.Count);
        }

        [TestMethod]
        //take larger than available returns remaining
        public async Task GetPatientsPageAsync_TakeGreaterThanCount_ReturnsRemaining()
        {
            using var scope = _provider.CreateScope();
            var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var maxId = GetMaxId();
            _db.Patients.AddRange(
                new PatientModel { Id = ++maxId, Name = "A" },
                new PatientModel { Id = ++maxId, Name = "B" });

            await _db.SaveChangesAsync();

            var result = await _service.GetPatientsPageAsync(0, 10);

            Assert.AreEqual(2, result.Result.Count);
        }

        [TestMethod]
        //add new patients
        public async Task UpsertPatientsBatchAsync_InsertsNewPatients()
        {
            var maxId = GetMaxId();
            var models = new[]
            {
                new PatientModel { Id = ++maxId, Name = "John" },
                new PatientModel { Id = ++maxId, Name = "Kate" }
            };

            await _service.UpsertPatientsBatchAsync(models);

            using var scope = _provider.CreateScope();
            var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.AreEqual(2, _db.Patients.Count());
        }

        [TestMethod]
        //updates existing
        public async Task UpsertPatientsBatchAsync_UpdatesExisting()
        {
            var oldId = 0;
            using (var scope = _provider.CreateScope())
            {
                var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var maxId = GetMaxId();
                var oldPatient = new PatientModel { Id = ++maxId, Name = "Old" };
                oldId = oldPatient.Id;
                _db.Patients.Add(oldPatient);
                await _db.SaveChangesAsync();


                var models = new[]
                {
                   new PatientModel { Id = oldPatient.Id, Name = "New" }
                };
                await _service.UpsertPatientsBatchAsync(models);
            }

            //reload db context
            using (var scope = _provider.CreateScope())
            {
                var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var patient = _db.Patients.FirstOrDefault(s => s.Id == oldId);

                Assert.IsNotNull(patient, $"can not find patient with id: {oldId}");
                Assert.AreEqual("New", patient.Name);
            }
           
        }

        [TestMethod]
        //mixed insert + update
        public async Task UpsertPatientsBatchAsync_Mixed_InsertAndUpdate()
        {
            var maxId = GetMaxId();
            var old = new PatientModel { Id = maxId + 1, Name = "Old" };
            var guid = Guid.NewGuid();
            var updatedName = $"Updated_{guid}";
            var newName = $"New_{guid}";

            using (var scope = _provider.CreateScope())
            {
                var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                _db.Patients.Add(old);
                await _db.SaveChangesAsync();
                
                var models = new[]
                {
                  new PatientModel { Id = old.Id, Name = updatedName },
                  new PatientModel { Name = newName }
                };
                await _service.UpsertPatientsBatchAsync(models);
            }
            //refresh dbcontext
            using (var scope = _provider.CreateScope())
            {

                var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var newPatient = _db.Patients.FirstOrDefault(s => s.Name == newName);
                var updatedPatient = _db.Patients.FirstOrDefault(s => s.Id == old.Id);

                Assert.IsNotNull(newPatient, $"can not find new patient with name: {newName}");
                Assert.IsNotNull(updatedPatient, $"can not find updated patient with id: {old.Id}");
            }
        }

        private int GetMaxId()
        {
            var _db = _provider.GetRequiredService<AppDbContext>();
            return _db.Patients.Any() ? _db.Patients.Max(s => s.Id) : 0;
        }

        
    }
}
