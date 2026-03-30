using CommunityToolkit.Maui.Views;

namespace AclsTracker.Views;

public partial class PatientDataPopup : Popup
{
    public record PatientDataResult(string Nombre, string Apellido, string DNI);

    public PatientDataPopup()
    {
        InitializeComponent();
    }

    private void OnGuardarClicked(object sender, EventArgs e)
    {
        var nombre = string.IsNullOrWhiteSpace(NombreEntry.Text) ? "SIN NOMBRE" : NombreEntry.Text.Trim();
        var apellido = string.IsNullOrWhiteSpace(ApellidoEntry.Text) ? "SIN NOMBRE" : ApellidoEntry.Text.Trim();
        var dni = string.IsNullOrWhiteSpace(DniEntry.Text) ? "SIN DNI" : DniEntry.Text.Trim();

        Close(new PatientDataResult(nombre, apellido, dni));
    }

    private void OnOmitirClicked(object sender, EventArgs e)
    {
        Close(new PatientDataResult("SIN NOMBRE", "SIN NOMBRE", "SIN DNI"));
    }
}
