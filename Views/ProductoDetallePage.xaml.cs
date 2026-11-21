using System.Globalization;
using CatalogoMaui.Models;
using CatalogoMaui.Services;

namespace CatalogoMaui.Views;

public partial class ProductoDetallePage : ContentPage, IQueryAttributable
{
	private int id;
	private string fotoActual = "mochila.jpg";
	private string? fotoTemporal;

	public ProductoDetallePage()
	{
		InitializeComponent();
		Title = "Agregar producto";

		Foto.ItemsSource = new List<string>
		{
			"mochila.jpg",
			"libreta.jpg",
			"termo.jpg"
		};

		Foto.SelectedIndex = 0;
	}

	// Con datos es edición. Sin datos conservamos el formulario nuevo.
	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (!query.TryGetValue("producto", out var dato) || dato is not Producto p)
			return;

		id = p.Id;
		Title = "Editar producto";

		Nombre.Text = p.Nombre;
		Descripcion.Text = p.Descripcion;
		Detalle.Text = p.Detalle;
		Precio.Text = p.Precio.ToString("0.00", CultureInfo.CurrentCulture);

		fotoActual = p.Foto;
		Foto.SelectedItem = p.Foto;
		VistaFoto.Source = FotosProducto.Resolver(p.Foto);

		Eliminar.IsVisible = true;
	}

	private void AlCambiarFoto(object? sender, EventArgs e)
	{
		if (Foto.SelectedItem is string archivo)
		{
			fotoActual = archivo;
			fotoTemporal = null;
			VistaFoto.Source = archivo;
		}
	}

	private async void AlElegirFoto(object? sender, EventArgs e)
	{
		var ruta = await FotosProducto.ElegirAsync();

		if (ruta == null)
			return;

		fotoTemporal = ruta;
		VistaFoto.Source = ImageSource.FromFile(ruta);
	}

	private async void AlGuardar(object? sender, EventArgs e)
	{
		if (!Guardar.IsEnabled) return;

		var textoPrecio = Precio.Text?.Trim().Replace(',', '.');

		if (string.IsNullOrWhiteSpace(Nombre.Text) ||
			string.IsNullOrWhiteSpace(Descripcion.Text) ||
			string.IsNullOrWhiteSpace(Detalle.Text) ||
			!decimal.TryParse(textoPrecio, NumberStyles.AllowDecimalPoint,
			CultureInfo.InvariantCulture, out var importe) ||
			importe < 0 || importe > 99999999.99m ||
			decimal.Round(importe, 2) != importe)
		{
			await DisplayAlert("Revisa los datos",
				"Completa los campos y escribe un precio válido de hasta dos decimales.",
				"Aceptar");
			return;
		}

		await EjecutarAsync(async () =>
		{
			string fotoFinal = fotoActual;
			string? fotoNueva = null;

			// Si se eligió una foto de la galería, guardamos su copia.
			if (!string.IsNullOrEmpty(fotoTemporal))
			{
				fotoNueva = await FotosProducto.GuardarAsync(fotoTemporal);

				if (fotoNueva == null)
					throw new InvalidOperationException("No se pudo guardar la imagen.");

				fotoFinal = fotoNueva;
			}

			var producto = new Producto
			{
				Id = id,
				Nombre = Nombre.Text.Trim(),
				Descripcion = Descripcion.Text.Trim(),
				Detalle = Detalle.Text.Trim(),
				Precio = importe,
				Foto = fotoFinal
			};

			try
			{
				// Id cero: todavía no hay fila. Otro Id: actualizamos esa fila.
				if (id == 0)
					await ProductosSqlite.Instancia.Insertar(producto);
				else if (!await ProductosSqlite.Instancia.Actualizar(producto))
					throw new InvalidOperationException("El producto ya no existe.");
			}
			catch
			{
				// Si falla SQLite, eliminamos la foto nueva.
				if (fotoNueva != null)
					FotosProducto.Eliminar(fotoNueva);

				throw;
			}

			// Si todo salió bien, retiramos la foto anterior.
			if (fotoNueva != null && id != 0 && fotoActual != fotoNueva)
				FotosProducto.Eliminar(fotoActual);

			fotoActual = fotoFinal;
			fotoTemporal = null;

			return true;
		});
	}

	private async void AlEliminar(object? sender, EventArgs e)
	{
		if (!Eliminar.IsEnabled || id == 0) return;

		await EjecutarAsync(async () =>
		{
			if (!await DisplayAlert("Eliminar producto",
				"¿Deseas eliminar este producto?",
				"Eliminar", "Cancelar"))
				return false;

			if (!await ProductosSqlite.Instancia.Eliminar(id))
				throw new InvalidOperationException("El producto ya no existe.");

			return true;
		});
	}

	private async Task EjecutarAsync(Func<Task<bool>> operacion)
	{
		Guardar.IsEnabled = Eliminar.IsEnabled = false;

		try
		{
			if (await operacion())
				await Shell.Current.GoToAsync("..");
		}
		catch (Exception ex)
		{
			await DisplayAlert("No se completó la operación",
				ex.Message, "Aceptar");
		}
		finally
		{
			Guardar.IsEnabled = Eliminar.IsEnabled = true;
		}
	}
}