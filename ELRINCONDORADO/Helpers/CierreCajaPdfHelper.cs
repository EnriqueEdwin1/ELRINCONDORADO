using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Helpers
{
    public static class CierreCajaPdfHelper
    {
        private static byte[]? _logoBytes;

        private static byte[] ObtenerLogo()
        {
            if (_logoBytes != null) return _logoBytes;
            var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo.png");
            _logoBytes = File.Exists(ruta) ? File.ReadAllBytes(ruta) : Array.Empty<byte>();
            return _logoBytes;
        }

        // Reporte de cierre de caja en HOJA CARTA (Letter) con todas las ventas del día
        public static byte[] Generar(CierreCajaViewModel vm)
        {
            var logo = ObtenerLogo();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(16, Unit.Millimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                    page.Header().Column(header =>
                    {
                        header.Item().Row(row =>
                        {
                            if (logo.Length > 0)
                                row.ConstantItem(90).AlignLeft().Image(logo).FitWidth();
                            row.RelativeItem().Column(enc =>
                        {
                            enc.Item().AlignCenter().Text("EL RINCÓN DORADO")
                                .FontSize(18).Bold().FontColor(Colors.Amber.Darken2);
                            enc.Item().AlignCenter().Text("REPORTE DE CIERRE DE CAJA")
                                .FontSize(13).Bold();
                            enc.Item().AlignCenter()
                                .Text($"Cajero: {vm.Cajero}   |   Fecha: {vm.Fecha.ToString("dd/MM/yyyy HH:mm")}")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                        });
                        header.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Amber.Darken2);
                    });

                    page.Content().PaddingVertical(8).Column(columna =>
                    {
                        if (vm.Ventas.Count == 0)
                        {
                            columna.Item().AlignCenter().PaddingVertical(20)
                                .Text("No se registraron ventas este día.").FontColor(Colors.Grey.Darken1);
                        }
                        else
                        {
                            columna.Item().Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columnas =>
                                {
                                    columnas.ConstantColumn(26);
                                    columnas.ConstantColumn(48);
                                    columnas.ConstantColumn(48);
                                    columnas.RelativeColumn(2);
                                    columnas.ConstantColumn(64);
                                    columnas.ConstantColumn(52);
                                    columnas.ConstantColumn(64);
                                });

                                tabla.Header(th =>
                                {
                                    th.Cell().Padding(3).Text("Factura").Bold().FontSize(8.5f);
                                    th.Cell().Padding(3).Text("Hora").Bold().FontSize(8.5f);
                                    th.Cell().Padding(3).Text("Tipo").Bold().FontSize(8.5f);
                                    th.Cell().Padding(3).Text("Cliente").Bold().FontSize(8.5f);
                                    th.Cell().Padding(3).Text("Pago").Bold().FontSize(8.5f);
                                    th.Cell().Padding(3).Text("Estado").Bold().FontSize(8.5f);
                                    th.Cell().Padding(3).AlignRight().Text("Total").Bold().FontSize(8.5f);
                                });

                                foreach (var venta in vm.Ventas)
                                {
                                    var tipo = venta.TipoPedido == "MESA"
                                        ? (venta.Mesa != null ? $"Mesa {venta.Mesa.Numero}" : "Mesa")
                                        : "Llevar";
                                    var cliente = !string.IsNullOrWhiteSpace(venta.NombrePedido)
                                        ? venta.NombrePedido
                                        : (!string.IsNullOrWhiteSpace(venta.Cliente?.RazonSocial)
                                            ? venta.Cliente.RazonSocial
                                            : "—");

                                    tabla.Cell().Padding(3).Text(venta.NumeroPedido ?? $"{venta.IdPedido:D4}").FontSize(8.5f);
                                    tabla.Cell().Padding(3).Text(venta.FechaCreacion.ToLocalTime().ToString("HH:mm")).FontSize(8.5f);
                                    tabla.Cell().Padding(3).Text(tipo).FontSize(8.5f);
                                    tabla.Cell().Padding(3).Text(cliente).FontSize(8.5f);
                                    tabla.Cell().Padding(3).Text(venta.EstadoPago).FontSize(8.5f);
                                    tabla.Cell().Padding(3).Text(venta.Estado).FontSize(8.5f);
                                    tabla.Cell().Padding(3).AlignRight().Text(MonedaHelper.FormatearBs(venta.Total)).FontSize(8.5f);
                                }
                            });
                        }

                        columna.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                        // ===== Resumen por método de pago =====
                        columna.Item().PaddingTop(4).Table(resumen =>
                        {
                            resumen.ColumnsDefinition(c2 =>
                            {
                                c2.RelativeColumn();
                                c2.ConstantColumn(150);
                            });

                            resumen.Cell().Column(det =>
                            {
                                det.Item().Text("RESUMEN DEL DÍA").Bold().FontColor(Colors.Amber.Darken2).FontSize(9);
                                det.Item().PaddingTop(2).Text($"Cantidad de ventas: {vm.Cantidad}").FontSize(9);
                                det.Item().Text($"Subtotal: {MonedaHelper.FormatearBs(vm.Subtotal)}").FontSize(9);
                            });

                            resumen.Cell().Column(cuadro =>
                            {
                                cuadro.Item().Border(0.75f).BorderColor(Colors.Amber.Darken2).Padding(8).Column(inner =>
                                {
                                    inner.Item().Row(r1 =>
                                    {
                                        r1.RelativeItem().Text("Efectivo:").FontSize(9);
                                        r1.ConstantItem(70).AlignRight().Text(MonedaHelper.FormatearBs(vm.Efectivo)).FontSize(9);
                                    });
                                    inner.Item().Row(r2 =>
                                    {
                                        r2.RelativeItem().Text("Tarjeta:").FontSize(9);
                                        r2.ConstantItem(70).AlignRight().Text(MonedaHelper.FormatearBs(vm.Tarjeta)).FontSize(9);
                                    });
                                    inner.Item().Row(r3 =>
                                    {
                                        r3.RelativeItem().Text("QR:").FontSize(9);
                                        r3.ConstantItem(70).AlignRight().Text(MonedaHelper.FormatearBs(vm.QR)).FontSize(9);
                                    });
                                    inner.Item().PaddingTop(3).LineHorizontal(0.75f).LineColor(Colors.Grey.Darken1);
                                    inner.Item().PaddingTop(3).Row(r4 =>
                                    {
                                        r4.RelativeItem().Text("TOTAL:").Bold().FontSize(11);
                                        r4.ConstantItem(72).AlignRight().Text(MonedaHelper.FormatearBs(vm.Total))
                                            .Bold().FontSize(11).FontColor(Colors.Amber.Darken2);
                                    });
                                });
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text("El Rincón Dorado – Confianza y rendimiento")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });
            }).GeneratePdf();
        }
    }
}