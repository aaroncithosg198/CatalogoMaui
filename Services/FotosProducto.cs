
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace CatalogoMaui.Services
{
	public static class FotosProducto
	{
		public static async Task<string> ElegirAsync()
		{
			try
			{
				var resultado = await FilePicker.Default.PickAsync(
					new PickOptions
					{
						PickerTitle = "Selecciona una imagen",
						FileTypes = FilePickerFileType.Images
					});

				if (resultado == null)
					return null;

				string extension = Path.GetExtension(resultado.FileName).ToLower();

				if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
					return null;

				using var stream = await resultado.OpenReadAsync();

				if (stream.Length > 15 * 1024 * 1024)
					return null;

				string nombre = "temporal_" + Guid.NewGuid().ToString("N") + extension;
				string ruta = Path.Combine(FileSystem.CacheDirectory, nombre);

				using var archivo = File.Create(ruta);
				await stream.CopyToAsync(archivo);

				return ruta;
			}
			catch
			{
				return null;
			}
		}

		public static async Task<string> GuardarAsync(string rutaTemporal)
		{
			if (string.IsNullOrEmpty(rutaTemporal) || !File.Exists(rutaTemporal))
				return null;

			string carpeta = Path.Combine(
				FileSystem.AppDataDirectory,
				"fotos_productos");

			Directory.CreateDirectory(carpeta);

			string extension = Path.GetExtension(rutaTemporal);
			string nombre = "producto_" + Guid.NewGuid().ToString("N") + extension;

			string destino = Path.Combine(carpeta, nombre);

			using var origen = File.OpenRead(rutaTemporal);
			using var destinoArchivo = File.Create(destino);

			await origen.CopyToAsync(destinoArchivo);

			return nombre;
		}

		public static string Resolver(string foto)
		{
			if (string.IsNullOrEmpty(foto))
				return null;

			if (foto.StartsWith("producto_"))
			{
				return Path.Combine(
					FileSystem.AppDataDirectory,
					"fotos_productos",
					foto);
			}

			return foto;
		}

		public static void Eliminar(string foto)
		{
			if (string.IsNullOrEmpty(foto))
				return;

			if (foto.StartsWith("producto_"))
			{
				string ruta = Path.Combine(
					FileSystem.AppDataDirectory,
					"fotos_productos",
					foto);

				if (File.Exists(ruta))
					File.Delete(ruta);
			}
		}
	}
}

