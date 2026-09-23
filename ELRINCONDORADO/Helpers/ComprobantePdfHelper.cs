using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ELRINCONDORADO.Models;

namespace ELRINCONDORADO.Helpers
{
    public static class ComprobantePdfHelper
    {
        public static byte[] Generar(Compra compra)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                    page.Header().Element(encabezado => ComposeHeader(encabezado, compra));
                    page.Content().Element(contenido => ComposeContent(contenido, compra));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.DefaultTextStyle(s => s.FontSize(8).FontColor(Colors.Grey.Medium));
                        x.Span("El Rincón Dorado — Comprobante de compra · Página ");
                        x.CurrentPageNumber().FontColor(Colors.Grey.Medium);
                    });
                });
            }).GeneratePdf();
        }

        private static void ComposeHeader(IContainer container, Compra compra)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("EL RINCÓN DORADO")
                        .FontSize(18).Bold().FontColor(Colors.Amber.Darken2);
                    col.Item().Text("Comprobante de compra")
                        .FontSize(12).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(170).AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text($"Nº {compra.IdCompra:D4}")
                        .FontSize(14).Bold();
                    col.Item().AlignRight().Text($"Fecha: {compra.FechaCompra:dd/MM/yyyy HH:mm}");
                    col.Item().AlignRight().Text($"Estado: {compra.Estado}")
                        .FontColor(Colors.Green.Darken1);
                });
            });
        }

        private static void ComposeContent(IContainer container, Compra compra)
        {
            container.PaddingVertical(12).Column(col =>
            {
                col.Item().PaddingBottom(8).Row(row =>
                {
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                    {
                        c.Item().Text("PROVEEDOR").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                        c.Item().Text(compra.Proveedor?.Nombre ?? "—").FontSize(11).SemiBold();
                        c.Item().Text($"Tel: {compra.Proveedor?.Telefono ?? "—"}").FontSize(9);
                    });
                    row.ConstantItem(10);
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                    {
                        c.Item().Text("REALIZADA POR").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                        c.Item().Text(compra.Empleado != null ? $"{compra.Empleado.Nombre} {compra.Empleado.Apellido}" : "—")
                            .FontSize(11).SemiBold();
                        c.Item().Text($"Usuario: {compra.Empleado?.Usuario ?? "—"}").FontSize(9);
                    });
                });

                col.Item().Table(tabla =>
                {
                    tabla.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(45);
                        columns.ConstantColumn(70);
                        columns.ConstantColumn(70);
                    });

                    tabla.Header(header =>
                    {
                        header.Cell().Background(Colors.Amber.Darken2).Padding(5)
                            .Text("Insumo").FontColor(Colors.White).FontSize(9).Bold();
                        header.Cell().Background(Colors.Amber.Darken2).Padding(5).AlignRight()
                            .Text("Cantidad").FontColor(Colors.White).FontSize(9).Bold();
                        header.Cell().Background(Colors.Amber.Darken2).Padding(5).AlignRight()
                            .Text("Costo Unit.").FontColor(Colors.White).FontSize(9).Bold();
                        header.Cell().Background(Colors.Amber.Darken2).Padding(5).AlignRight()
                            .Text("Subtotal").FontColor(Colors.White).FontSize(9).Bold();
                    });

                    foreach (var detalle in compra.DetallesCompras ?? new List<DetalleCompra>())
                    {
                        var fila = (compra.DetallesCompras?.ToList().IndexOf(detalle) ?? 0) % 2 == 0
                            ? Colors.Grey.Lighten4
                            : Colors.White;

                        tabla.Cell().Background(fila).Padding(5)
                            .Text(detalle.Insumo?.Nombre ?? "—").FontSize(9);
                        tabla.Cell().Background(fila).Padding(5).AlignRight()
                            .Text($"{detalle.Cantidad:N2} {detalle.Insumo?.UnidadMedida ?? ""}").FontSize(9);
                        tabla.Cell().Background(fila).Padding(5).AlignRight()
                            .Text(MonedaHelper.FormatearBs(detalle.CostoUnitario)).FontSize(9);
                        tabla.Cell().Background(fila).Padding(5).AlignRight()
                            .Text(MonedaHelper.FormatearBs(detalle.Subtotal)).FontSize(9);
                    }
                });

                col.Item().PaddingTop(10).AlignRight().Column(total =>
                {
                    total.Item().Row(r =>
                    {
                        r.RelativeItem().AlignRight().Text("Subtotal:").FontSize(10);
                        r.ConstantItem(90).AlignRight().Text(MonedaHelper.FormatearBs(compra.Subtotal)).FontSize(10);
                    });
                    total.Item().Row(r =>
                    {
                        r.RelativeItem().AlignRight().Text("Descuento:").FontSize(10);
                        r.ConstantItem(90).AlignRight().Text($"- {MonedaHelper.FormatearBs(compra.Descuento)}").FontSize(10);
                    });
                    total.Item().PaddingTop(6).Row(r =>
                    {
                        r.RelativeItem().AlignRight().Text("TOTAL:").FontSize(13).Bold();
                        r.ConstantItem(90).AlignRight().Text(MonedaHelper.FormatearBs(compra.Total))
                            .FontSize(13).Bold().FontColor(Colors.Amber.Darken2);
                    });
                });
            });
        }
    }
}