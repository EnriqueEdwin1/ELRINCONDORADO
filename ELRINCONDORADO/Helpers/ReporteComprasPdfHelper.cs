using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Helpers
{
    public static class ReporteComprasPdfHelper
    {
        public static byte[] Generar(List<Compra> compras, DateTime inicio, DateTime fin, string titulo)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                    page.Header().Element(c => ComposeHeader(c, titulo, inicio, fin));
                    page.Content().Element(c => ComposeContent(c, compras, inicio, fin));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.DefaultTextStyle(s => s.FontSize(8).FontColor(Colors.Grey.Medium));
                        x.Span("El Rincón Dorado — Reporte de compras · Página ");
                        x.CurrentPageNumber().FontColor(Colors.Grey.Medium);
                    });
                });
            }).GeneratePdf();
        }

        private static void ComposeHeader(IContainer container, string titulo, DateTime inicio, DateTime fin)
        {
            var esMes = inicio.Day == 1 && fin.Date == inicio.AddMonths(1).AddDays(-1);
            var rango = esMes
                ? $"Del {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}"
                : $"El {inicio:dd/MM/yyyy}";

            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("EL RINCÓN DORADO")
                        .FontSize(18).Bold().FontColor(Colors.Amber.Darken2);
                    col.Item().Text(titulo)
                        .FontSize(12).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(240).AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text("Reporte de compras")
                        .FontSize(14).Bold();
                    col.Item().AlignRight().Text(rango)
                        .FontSize(9);
                    col.Item().AlignRight().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private static void ComposeContent(IContainer container, List<Compra> compras, DateTime inicio, DateTime fin)
        {
            container.Column(col =>
            {
                col.Item().PaddingVertical(12).Row(row =>
                {
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                    {
                        c.Item().Text("COMPRAS").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                        c.Item().AlignRight().Text(compras.Count.ToString()).FontSize(18).Bold();
                    });
                    row.ConstantItem(10);
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                    {
                        c.Item().Text("PROVEEDORES").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                        c.Item().AlignRight().Text(compras.Select(cr => cr.IdProveedor).Distinct().Count().ToString())
                            .FontSize(18).Bold();
                    });
                    row.ConstantItem(10);
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Background(Colors.Amber.Lighten5).Column(c =>
                    {
                        c.Item().Text("TOTAL PERÍODO").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                        c.Item().AlignRight().Text(MonedaHelper.FormatearBs(compras.Sum(cr => cr.Total)))
                            .FontSize(15).Bold().FontColor(Colors.Amber.Darken2);
                    });
                });

                if (compras.Count == 0)
                {
                    col.Item().PaddingTop(20).AlignCenter().Text("No se registraron compras en el período seleccionado.")
                        .FontSize(11).FontColor(Colors.Grey.Darken1);
                    return;
                }

                foreach (var compra in compras)
                {
                    col.Item().PaddingTop(12).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(bloque =>
                    {
                        var empleado = compra.Empleado != null
                            ? $"{compra.Empleado.Nombre} {compra.Empleado.Apellido}"
                            : "—";

                        bloque.Item().Row(r =>
                        {
                            r.RelativeItem().Text($"Compra Nº {compra.IdCompra:D4}")
                                .FontSize(11).Bold().FontColor(Colors.Amber.Darken2);
                            r.ConstantItem(210).AlignRight()
                                .Text($"Fecha: {compra.FechaCompra:dd/MM/yyyy HH:mm}").FontSize(9);
                        });
                        bloque.Item().PaddingTop(2)
                            .Text($"Proveedor: {compra.Proveedor?.Nombre ?? "—"} · Realizada por: {empleado}")
                            .FontSize(9);

                        bloque.Item().PaddingTop(6).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(65);
                                columns.ConstantColumn(70);
                            });

                            tabla.Header(header =>
                            {
                                header.Cell().Background(Colors.Amber.Darken2).Padding(4)
                                    .Text("Insumo").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(Colors.Amber.Darken2).Padding(4).AlignRight()
                                    .Text("Cantidad").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(Colors.Amber.Darken2).Padding(4).AlignRight()
                                    .Text("Costo Unit.").FontColor(Colors.White).FontSize(8).Bold();
                                header.Cell().Background(Colors.Amber.Darken2).Padding(4).AlignRight()
                                    .Text("Subtotal").FontColor(Colors.White).FontSize(8).Bold();
                            });

                            var detalles = (compra.DetallesCompras ?? new List<DetalleCompra>()).ToList();
                            for (var i = 0; i < detalles.Count; i++)
                            {
                                var detalle = detalles[i];
                                var fila = i % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                                tabla.Cell().Background(fila).Padding(4)
                                    .Text(detalle.Insumo?.Nombre ?? "—").FontSize(8);
                                tabla.Cell().Background(fila).Padding(4).AlignRight()
                                    .Text($"{MonedaHelper.FormatearNumero(detalle.Cantidad)} {detalle.Insumo?.UnidadMedida ?? ""}").FontSize(8);
                                tabla.Cell().Background(fila).Padding(4).AlignRight()
                                    .Text(MonedaHelper.FormatearBs(detalle.CostoUnitario)).FontSize(8);
                                tabla.Cell().Background(fila).Padding(4).AlignRight()
                                    .Text(MonedaHelper.FormatearBs(detalle.Subtotal)).FontSize(8);
                            }
                        });

                        bloque.Item().PaddingTop(6).AlignRight().Column(total =>
                        {
                            total.Item().Text($"Subtotal: {MonedaHelper.FormatearBs(compra.Subtotal)}").FontSize(9);
                            if (compra.Descuento > 0)
                                total.Item().Text($"Descuento: - {MonedaHelper.FormatearBs(compra.Descuento)}").FontSize(9);
                            total.Item().Text($"TOTAL COMPRA: {MonedaHelper.FormatearBs(compra.Total)}")
                                .FontSize(11).Bold();
                        });
                    });
                }

                col.Item().PaddingTop(14).BorderTop(1).BorderColor(Colors.Grey.Lighten2).AlignRight().Column(totalFinal =>
                {
                    totalFinal.Item().PaddingTop(6).AlignRight().Text($"TOTAL GENERAL DEL PERÍODO: {MonedaHelper.FormatearBs(compras.Sum(c => c.Total))}")
                        .FontSize(13).Bold().FontColor(Colors.Amber.Darken2);
                });
            });
        }
    }
}