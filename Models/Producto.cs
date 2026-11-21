using CatalogoMaui.Services;
using SQLite;
namespace CatalogoMaui.Models;

[Table("Productos")]
public class Producto
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }
	public string Nombre { get; set; } = "";
	public string Descripcion { get; set; } = "";
	// Guardamos centavos en un entero para evitar redondeos en SQLite.
	public long PrecioCentavos { get; set; }
	[Ignore]
	public decimal Precio
	{
		get => PrecioCentavos / 100m;
		set => PrecioCentavos = (long)(value * 100m);
	}
	public string Detalle { get; set; } = "";
	// Nombre de una imagen incluida en Resources/Images.
	public string Foto { get; set; } = "";
	
	[Ignore]
	public string FotoVisible => FotosProducto.Resolver(Foto);
}