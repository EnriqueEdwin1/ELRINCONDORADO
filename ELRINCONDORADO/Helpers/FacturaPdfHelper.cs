using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Helpers
{
    public static class FacturaPdfHelper
    {
        private static byte[]? _logoBytes;

        // Carga el logo del restaurante desde wwwroot/img/logo.png
        private static byte[] ObtenerLogo()
        {
            if (_logoBytes != null) return _logoBytes;
            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo.png");
            _logoBytes = File.Exists(ruta) ? File.ReadAllBytes(ruta) : Array.Empty<byte>();
            return _logoBytes;
        }

        // Genera la factura de un Pedido (venta en mostrador) en formato TICKET:
        // una hoja rectangular angosta estilo recibo de supermercado (~80 mm de ancho),
        // con el logo del restaurante en el encabezado.
        public static byte[] Generar(Pedido pedido)
        {
            var subtotal = pedido.DetallesPedidos?.Sum(d => d.Subtotal) ?? 0;
            var total = pedido.Total > 0 ? pedido.Total : (subtotal - pedido.Descuento);
            var detalles = (pedido.DetallesPedidos ?? new List<DetallePedido>()).ToList();
            var logo = ObtenerLogo();

            // Altura dinámica según el número de líneas (mm)
            var alto = Math.Clamp(128f + detalles.Count * 6.6f, 148f, 900f);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(80, alto, Unit.Millimetre);
                    page.Margin(2f, Unit.Millimetre);
                    page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Black));

                    page.Content().Padding(1, Unit.Millimetre).Column(col =>
                    {
                        // ===== Encabezado: logo + tienda =====
                        if (logo.Length > 0)
                            col.Item().AlignCenter().Width(52, Unit.Millimetre).PaddingTop(2).Image(logo).FitWidth();

                        col.Item().PaddingTop(3).AlignCenter().Text("EL RINCÓN DORADO")
                            .FontSize(15).Bold().FontColor(Colors.Amber.Darken2);
                        col.Item().AlignCenter().Text("FACTURA DE VENTA")
                            .FontSize(10).Bold();
                        Separador(col);

                        // ===== Datos de la factura y del cliente =====
                        LineaInfo(col, "Nº Factura:", pedido.NumeroPedido ?? $"{pedido.IdPedido:D4}");
                        LineaInfo(col, "Fecha:", pedido.FechaCreacion.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
                        LineaInfo(col, "Tipo:", NombreTipo(pedido));
                        LineaInfo(col, "Estado:", pedido.Estado);
                        LineaInfo(col, "Pago:", pedido.EstadoPago);
                        LineaInfo(col, "NIT/CI:", pedido.Cliente?.Nit ?? "-");
                        LineaInfo(col, "Cliente:", pedido.Cliente?.RazonSocial ?? "SIN NOMBRE");
                        Separador(col);

                        // ===== Detalle de productos =====
                        col.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columnas =>
                            {
                                columnas.RelativeColumn();
                                columnas.ConstantColumn(26);
                                columnas.ConstantColumn(46);
                                columnas.ConstantColumn(56);
                            });

                            tabla.Header(header =>
                            {
                                header.Cell().Text("Producto").FontSize(8).Bold();
                                header.Cell().AlignRight().Text("Cant").FontSize(8).Bold();
                                header.Cell().AlignRight().Text("P.Unit").FontSize(8).Bold();
                                header.Cell().AlignRight().Text("Subtotal").FontSize(8).Bold();
                            });

                            foreach (var detalle in detalles)
                            {
                                tabla.Cell().Text(detalle.Producto?.Nombre ?? "-").FontSize(8);
                                tabla.Cell().AlignRight().Text(detalle.Cantidad.ToString()).FontSize(8);
                                tabla.Cell().AlignRight().Text(MonedaHelper.FormatearBs(detalle.PrecioUnitario)).FontSize(8);
                                tabla.Cell().AlignRight().Text(MonedaHelper.FormatearBs(detalle.Subtotal)).FontSize(8);
                            }
                        });
                        Separador(col);

                        // ===== Totales =====
                        LineaDerecha(col, "Subtotal:", MonedaHelper.FormatearBs(subtotal));
                        if (pedido.Descuento > 0)
                            LineaDerecha(col, "Descuento:", MonedaHelper.FormatearBs(pedido.Descuento));
                        col.Item().PaddingTop(3).Row(r =>
                        {
                            r.RelativeItem().AlignRight().Text("TOTAL:").Bold().FontSize(12);
                            r.ConstantItem(74).AlignRight().Text(MonedaHelper.FormatearBs(total))
                                .Bold().FontSize(12).FontColor(Colors.Amber.Darken2);
                        });
                        Separador(col);

                        // ===== Pie del ticket =====
                        col.Item().PaddingTop(5).AlignCenter().Text("¡Gracias por su compra!")
                            .FontSize(10).Bold();
                        col.Item().AlignCenter().Text("El Rincón Dorado – Confianza y rendimiento")
                            .FontSize(7).FontColor(Colors.Grey.Medium);
                        var atendido = $"{pedido.Empleado?.Nombre ?? ""} {pedido.Empleado?.Apellido ?? ""}".Trim();
                        if (!string.IsNullOrWhiteSpace(atendido))
                            col.Item().AlignCenter().Text($"Atendido por: {atendido}")
                                .FontSize(7).FontColor(Colors.Grey.Medium);
                    });
                });
            }).GeneratePdf();
        }

        private static void Separador(ColumnDescriptor col)
        {
            col.Item().PaddingVertical(4).AlignCenter()
                .Text(new string('─', 34)).FontSize(8).FontColor(Colors.Grey.Lighten1);
        }

        private static string NombreTipo(Pedido pedido)
        {
            if (pedido.TipoPedido == "MESA")
                return pedido.Mesa != null ? $"Mesa {pedido.Mesa.Numero}" : "Servido en mesa";
            return "Para llevar";
        }

        private static void LineaInfo(ColumnDescriptor col, string etiqueta, string valor)
        {
            col.Item().Row(r =>
            {
                r.ConstantItem(58).Text(etiqueta).FontSize(8).FontColor(Colors.Grey.Darken2);
                r.RelativeItem().Text(valor).FontSize(9);
            });
        }

        private static void LineaDerecha(ColumnDescriptor col, string etiqueta, string valor)
        {
            col.Item().Row(r =>
            {
                r.RelativeItem().AlignRight().Text(etiqueta).FontSize(9);
                r.ConstantItem(60).AlignRight().Text(valor).FontSize(9);
            });
        }
    }
}