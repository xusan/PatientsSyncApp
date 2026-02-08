using Core.Contracts.Services;
using Core.Models;
using CsvHelper;
using EfDataStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.AppServices;

namespace IntegrationTest.AppServices
{
    [TestCategory("Integration")]
    [TestClass]
    public class SyncServiceTest : BaseAppServiceTest
    {
        [TestMethod]
        public async Task ImportPatients_CreatesPatientsInDb()
        {
            var file = Path.Combine(_dir, "patients.csv");

            await File.WriteAllTextAsync(file,
                                            "Id,Name,Surname,DateOfBirth,Email\n" +
                                            "1,John,Doe,2000-01-01,john@test.com");

            using var scope = _provider.CreateScope();
            var sync = scope.ServiceProvider.GetRequiredService<ISyncService>();

            var res = await sync.ImportPatientsFromCsvAsync(_dir);

            Assert.IsTrue(res.Success);


            using var scope2 = _provider.CreateScope();
            var db = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.AreEqual(1, db.Patients.Count());
        }


        [TestMethod]
        public async Task ImportPatients_UpdatesExisting()
        {
            int id = 1;

            // seed existing patient
            using (var scope = _provider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.Patients.Add(new PatientModel
                {
                    Id = id,
                    Name = "Old",
                    Surname = "OldS",
                    DateOfBirth = new DateTime(2000, 1, 1),
                    Email = "old@test.com"
                });

                await db.SaveChangesAsync();
            }

            // create CSV with updated values
            var file = Path.Combine(_dir, "patients.csv");

            await WritePatientsCsvAsync(file,
                new PatientModel
                {
                    Id = id,
                    Name = "New",
                    Surname = "NewS",
                    DateOfBirth = new DateTime(2001, 1, 1),
                    Email = "new@test.com"
                });

            // act
            using (var scope = _provider.CreateScope())
            {
                var sync = scope.ServiceProvider.GetRequiredService<ISyncService>();
                var res = await sync.ImportPatientsFromCsvAsync(_dir);

                Assert.IsTrue(res.Success);
            }

            // verify with fresh scope
            using (var scope = _provider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var patient = await db.Patients.FirstAsync(p => p.Id == id);

                Assert.AreEqual("New", patient.Name);
                Assert.AreEqual("NewS", patient.Surname);
                Assert.AreEqual("new@test.com", patient.Email);
            }
        }


        [TestMethod]
        public async Task ExportPatients_WritesCsvWithRows()
        {
            // seed DB
            using (var scope = _provider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.Patients.AddRange(
                    new PatientModel
                    {
                        Id = 1,
                        Name = "John",
                        Surname = "Doe",
                        DateOfBirth = new DateTime(2000, 1, 1),
                        Email = "john@test.com"
                    },
                    new PatientModel
                    {
                        Id = 2,
                        Name = "Kate",
                        Surname = "Smith",
                        DateOfBirth = new DateTime(1999, 5, 5),
                        Email = "kate@test.com"
                    });

                await db.SaveChangesAsync();
            }

            // act
            using (var scope = _provider.CreateScope())
            {
                var sync = scope.ServiceProvider.GetRequiredService<ISyncService>();
                var res = await sync.ExportPatientsToCsvAsync(_dir);

                Assert.IsTrue(res.Success);
            }

            // verify file content
            var file = Directory.GetFiles(_dir, "*.csv").Single();
            var text = await File.ReadAllTextAsync(file);

            // header exists
            StringAssert.Contains(text, "Name");
            StringAssert.Contains(text, "Surname");
            StringAssert.Contains(text, "Email");

            // data exists
            StringAssert.Contains(text, "John");
            StringAssert.Contains(text, "Kate");
            StringAssert.Contains(text, "john@test.com");
            StringAssert.Contains(text, "kate@test.com");
        }

        private async Task WritePatientsCsvAsync(string path, params PatientModel[] patients)
        {
            using var writer = new StreamWriter(path);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteHeader<PatientModel>();
            await csv.NextRecordAsync();
            csv.WriteRecords(patients);
        }
    }
}
