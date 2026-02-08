using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfApp.ViewModels;

public class PatientItemViewModel : ObservableObject
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; }
}
