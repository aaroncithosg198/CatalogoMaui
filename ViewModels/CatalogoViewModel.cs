using System.Collections.Generic;
using CatalogoMaui.Models;


namespace CatalogoMaui.ViewModels;

public class CatalogoViewModel
{
	// Cada objeto de esta lista se convertirá en una tarjeta.
	public List<Producto> Productos { get; } = new()
	{
		new Producto
		{
			Nombre = "Audífonos Bluetooth",
			Descripcion = "Audífonos inalámbricos con buen sonido.",
			Precio = 599.00m,
			Foto = "audif.jpg"
		},
		new Producto
		{
			Nombre = "Teclado mecánico",
			Descripcion = "Teclado para videojuegos y trabajo.",
			Precio = 899.00m,
			Foto = "teclado.jpg"
		},
		new Producto
		{
			Nombre = "Mouse gamer",
			Descripcion = "Mouse ergonómico con luces RGB.",
			Precio = 450.00m,
			Foto = "mouse.jpg"
		},
		new Producto
		{
			Nombre = "Bocina Bluetooth",
			Descripcion = "Bocina portátil con sonido estéreo.",
			Precio = 750.00m,
			Foto = "bocina.jpg"
		}
	};
}