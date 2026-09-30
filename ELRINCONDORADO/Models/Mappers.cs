using ELRINCONDORADO.Models.ApiDtos;

namespace ELRINCONDORADO.Models;

public static class Mappers
{
    // Categoria
    public static Categoria ToModel(this CategoriaDto dto)
    {
        return new Categoria
        {
            IdCategoria = dto.IdCategoria,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            CantidadProductos = dto.CantidadProductos,
            Productos = dto.Productos?.Select(p => p.ToModel()).ToList() ?? new List<Producto>()
        };
    }

    public static List<Categoria> ToModel(this IEnumerable<CategoriaDto>? dtos)
    {
        return dtos?.Select(d => d.ToModel()).ToList() ?? new List<Categoria>();
    }

    // Insumo
    public static Insumo ToModel(this InsumoDto dto)
    {
        return new Insumo
        {
            IdInsumo = dto.IdInsumo,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            UnidadMedida = dto.UnidadMedida,
            StockActual = dto.StockActual,
            StockMinimo = dto.StockMinimo,
            CostoUnitario = dto.CostoUnitario,
            Activo = dto.Activo,
            IdDestino = dto.IdDestino,
            DestinoNombre = dto.DestinoNombre,
            BajoMinimo = dto.BajoMinimo,
            Destino = dto.IdDestino.HasValue ? new DestinoInsumo
            {
                IdDestino = dto.IdDestino.Value,
                Nombre = dto.DestinoNombre ?? string.Empty
            } : null
        };
    }

    public static List<Insumo> ToModel(this IEnumerable<InsumoDto>? dtos)
    {
        return dtos?.Select(d => d.ToModel()).ToList() ?? new List<Insumo>();
    }

    // Producto
    public static Producto ToModel(this ProductoDto dto)
    {
        return new Producto
        {
            IdProducto = dto.IdProducto,
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            Activo = dto.Activo,
            ImagenUrl = dto.ImagenUrl,
            DisplayUrl = dto.DisplayUrl,
            IdCategoria = dto.IdCategoria,
            CategoriaNombre = dto.CategoriaNombre,
            TieneReceta = dto.TieneReceta
        };
    }

    public static Producto ToModel(this ProductoDetalleDto dto)
    {
        return new Producto
        {
            IdProducto = dto.IdProducto,
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            Activo = dto.Activo,
            ImagenUrl = dto.ImagenUrl,
            DisplayUrl = dto.DisplayUrl,
            IdCategoria = dto.IdCategoria,
            CategoriaNombre = dto.CategoriaNombre,
            TieneReceta = dto.TieneReceta,
            Receta = dto.Receta?.ToModel()
        };
    }

    public static List<Producto> ToModel(this IEnumerable<ProductoDto>? dtos)
    {
        return dtos?.Select(d => d.ToModel()).ToList() ?? new List<Producto>();
    }

    // ProductoRecetaViewModel (para Create/Edit de Productos)
    public static ProductoRecetaViewModel ToRecetaViewModel(this ProductoDto dto)
    {
        var detalleDto = dto as ProductoDetalleDto;
        var tieneReceta = detalleDto?.Receta != null;

        return new ProductoRecetaViewModel
        {
            IdProducto = dto.IdProducto,
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            Activo = dto.Activo,
            ImagenUrl = dto.ImagenUrl,
            DisplayUrl = dto.DisplayUrl,
            IdCategoria = dto.IdCategoria,
            CategoriaNombre = dto.CategoriaNombre,
            TieneReceta = dto.TieneReceta,
            Producto = new Producto
            {
                IdProducto = dto.IdProducto,
                Codigo = dto.Codigo,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                Activo = dto.Activo,
                ImagenUrl = dto.ImagenUrl,
                DisplayUrl = dto.DisplayUrl,
                IdCategoria = dto.IdCategoria,
                CategoriaNombre = dto.CategoriaNombre,
                TieneReceta = dto.TieneReceta
            },
            Receta = tieneReceta ? detalleDto.Receta.ToModel() : new Receta(),
            IncluirReceta = tieneReceta
        };
    }

    // Proveedor
    public static Proveedor ToModel(this ProveedorDto dto)
    {
        return new Proveedor
        {
            IdProveedor = dto.IdProveedor,
            Nombre = dto.Nombre,
            Telefono = dto.Telefono,
            Direccion = dto.Direccion,
            Email = dto.Email,
            Estado = dto.Estado,
            Observaciones = dto.Observaciones,
            TotalCompras = dto.TotalCompras,
            TotalGastado = dto.TotalGastado,
            UltimaCompra = dto.UltimaCompra
        };
    }

    public static List<Proveedor> ToModel(this IEnumerable<ProveedorDto>? dtos)
    {
        return dtos?.Select(d => d.ToModel()).ToList() ?? new List<Proveedor>();
    }

    // Receta
    public static Receta ToModel(this RecetaDto dto)
    {
        return new Receta
        {
            IdReceta = dto.IdReceta,
            IdProducto = dto.IdProducto,
            ProductoNombre = dto.ProductoNombre,
            Descripcion = dto.Descripcion,
            Activo = dto.Activo,
            Detalles = dto.Detalles?.Select(d => d.ToModel()).ToList() ?? new List<DetalleReceta>(),
            DetalleRecetas = dto.Detalles?.Select(d => d.ToModel()).ToList() ?? new List<DetalleReceta>(),
            Producto = new Producto
            {
                IdProducto = dto.IdProducto,
                Nombre = dto.ProductoNombre ?? string.Empty
            }
        };
    }

    public static List<Receta> ToModel(this IEnumerable<RecetaDto>? dtos)
    {
        return dtos?.Select(d => d.ToModel()).ToList() ?? new List<Receta>();
    }

    // RecetaViewModel (para Create/Edit de Recetas)
    public static RecetaViewModel ToViewModel(this RecetaDto dto)
    {
        return new RecetaViewModel
        {
            IdReceta = dto.IdReceta,
            IdProducto = dto.IdProducto,
            ProductoNombre = dto.ProductoNombre,
            Descripcion = dto.Descripcion,
            Activo = dto.Activo,
            Detalles = dto.Detalles?.Select(d => d.ToDetalleViewModel()).ToList() ?? new List<DetalleRecetaViewModel>()
        };
    }

    // DetalleReceta
    public static DetalleReceta ToModel(this RecetaDetalleDto dto)
    {
        return new DetalleReceta
        {
            IdDetalleReceta = dto.IdDetalleReceta,
            IdInsumo = dto.IdInsumo,
            InsumoNombre = dto.InsumoNombre,
            Cantidad = dto.Cantidad,
            UnidadMedida = dto.UnidadMedida
        };
    }

    public static DetalleRecetaViewModel ToDetalleViewModel(this RecetaDetalleDto dto)
    {
        return new DetalleRecetaViewModel
        {
            IdDetalleReceta = dto.IdDetalleReceta,
            IdInsumo = dto.IdInsumo,
            InsumoNombre = dto.InsumoNombre,
            Cantidad = dto.Cantidad,
            UnidadMedida = dto.UnidadMedida
        };
    }

    // MovimientoInventario
    public static MovimientoInventario ToModel(this MovimientoInventarioDto dto)
    {
        return new MovimientoInventario
        {
            IdMovimiento = dto.IdMovimiento,
            TipoMovimiento = dto.TipoMovimiento,
            Cantidad = dto.Cantidad,
            Fecha = dto.Fecha,
            Motivo = dto.Motivo,
            IdInsumo = dto.IdInsumo,
            InsumoNombre = dto.InsumoNombre,
            IdEmpleado = dto.IdEmpleado,
            EmpleadoNombre = dto.EmpleadoNombre,
            IdCierre = dto.IdCierre
        };
    }

    public static List<MovimientoInventario> ToModel(this IEnumerable<MovimientoInventarioDto>? dtos)
    {
        return dtos?.Select(d => d.ToModel()).ToList() ?? new List<MovimientoInventario>();
    }

    // Compra
    public static Compra ToModel(this CompraDto dto)
    {
        return new Compra
        {
            IdCompra = dto.IdCompra,
            IdProveedor = dto.IdProveedor,
            ProveedorNombre = dto.ProveedorNombre,
            Proveedor = new Proveedor
            {
                IdProveedor = dto.IdProveedor,
                Nombre = dto.ProveedorNombre ?? string.Empty
            },
            IdEmpleado = dto.IdEmpleado,
            EmpleadoNombre = dto.EmpleadoNombre,
            Empleado = new Empleado
            {
                IdEmpleado = dto.IdEmpleado,
                Nombre = dto.EmpleadoNombre ?? string.Empty
            },
            FechaCompra = dto.FechaCompra,
            Subtotal = dto.Subtotal,
            Descuento = dto.Descuento,
            Total = dto.Total,
            Estado = dto.Estado,
            Lineas = dto.Lineas,
            SumaDetalle = dto.SumaDetalle,
            SubtotalConsistente = dto.SubtotalConsistente,
            DetallesCompras = new List<DetalleCompra>()
        };
    }

    // CompraDetalleDto (para el detalle de una compra con sus items)
    public static Compra ToModel(this CompraDetalleDto dto)
    {
        var compra = new Compra
        {
            IdCompra = dto.IdCompra,
            IdProveedor = dto.IdProveedor,
            ProveedorNombre = dto.ProveedorNombre,
            Proveedor = new Proveedor
            {
                IdProveedor = dto.IdProveedor,
                Nombre = dto.ProveedorNombre ?? string.Empty
            },
            IdEmpleado = dto.IdEmpleado,
            EmpleadoNombre = dto.EmpleadoNombre,
            Empleado = new Empleado
            {
                IdEmpleado = dto.IdEmpleado,
                Nombre = dto.EmpleadoNombre ?? string.Empty
            },
            FechaCompra = dto.FechaCompra,
            Subtotal = dto.Subtotal,
            Descuento = dto.Descuento,
            Total = dto.Total,
            Estado = dto.Estado,
            Lineas = dto.Lineas,
            SumaDetalle = dto.SumaDetalle,
            SubtotalConsistente = dto.SubtotalConsistente,
            DetallesCompras = dto.Items?.Select(i => new DetalleCompra
            {
                IdDetalleCompra = i.IdDetalleCompra,
                IdInsumo = i.IdInsumo,
                InsumoNombre = i.InsumoNombre,
                InsumoActivo = i.InsumoActivo,
                Cantidad = i.Cantidad,
                CostoUnitario = i.CostoUnitario,
                Subtotal = i.Subtotal,
                CostoUnitarioActual = i.CostoUnitarioActual,
                CostoCambioDesdeCompra = i.CostoCambioDesdeCompra,
                EntradaRegistrada = i.EntradaRegistrada,
                Insumo = new Insumo
                {
                    IdInsumo = i.IdInsumo,
                    Nombre = i.InsumoNombre ?? string.Empty,
                    UnidadMedida = string.Empty,
                    CostoUnitario = i.CostoUnitarioActual ?? i.CostoUnitario
                }
            }).ToList() ?? new List<DetalleCompra>()
        };

        return compra;
    }

    public static List<Compra> ToModel(this IEnumerable<CompraDto>? dtos)
    {
        return dtos?.Select(d => d.ToModel()).ToList() ?? new List<Compra>();
    }

    // CompraViewModel (para Create de Compras)
    public static CompraViewModel ToViewModel(this CompraDto dto)
    {
        return new CompraViewModel
        {
            IdCompra = dto.IdCompra,
            IdProveedor = dto.IdProveedor,
            ProveedorNombre = dto.ProveedorNombre,
            IdEmpleado = dto.IdEmpleado,
            EmpleadoNombre = dto.EmpleadoNombre,
            FechaCompra = dto.FechaCompra,
            Subtotal = dto.Subtotal,
            Descuento = dto.Descuento,
            Total = dto.Total,
            Estado = dto.Estado
        };
    }

    // Pedido
    public static Pedido ToModel(this PedidoDto dto)
    {
        return new Pedido
        {
            IdPedido = dto.IdPedido,
            TipoPedido = dto.TipoPedido,
            Estado = dto.Estado,
            EstadoPago = dto.EstadoPago,
            FechaCreacion = dto.FechaCreacion,
            Subtotal = dto.Subtotal,
            Descuento = dto.Descuento,
            Total = dto.Total,
            NombrePedido = dto.NombrePedido,
            NumeroPedido = dto.NumeroPedido,
            Observaciones = dto.Observaciones,
            IdMesa = dto.IdMesa,
            NumeroMesa = dto.MesaNumero,
            IdEmpleado = dto.IdEmpleado,
            NombreCajero = dto.EmpleadoNombre,
            IdPromocion = dto.IdPromocion,
            NombrePromocion = dto.PromocionNombre,
            IdCliente = dto.IdCliente,
            NitCliente = null, // el NIT solo viaja en el detalle (PedidoDetalleDto)
            RazonSocialCliente = dto.ClienteRazonSocial,
            // Navegaciones: la API las devuelve aplanadas, se reconstruyen aquí.
            Mesa = dto.MesaNumero.HasValue
                ? new Mesa { IdMesa = dto.IdMesa ?? 0, Numero = dto.MesaNumero.Value }
                : null,
            Empleado = new Empleado { IdEmpleado = dto.IdEmpleado, Nombre = dto.EmpleadoNombre ?? string.Empty },
            Cliente = dto.IdCliente.HasValue || !string.IsNullOrWhiteSpace(dto.ClienteRazonSocial)
                ? new Cliente { IdCliente = dto.IdCliente ?? 0, RazonSocial = dto.ClienteRazonSocial }
                : null,
            Promocion = dto.IdPromocion.HasValue
                ? new PromocionListadoViewModel { IdPromocion = dto.IdPromocion.Value, Nombre = dto.PromocionNombre ?? string.Empty }
                : null,
            DetallesPedidos = new List<DetallePedido>()
        };
    }

    public static List<Pedido> ToModel(this IEnumerable<PedidoDto>? dtos)
    {
        return dtos?.Select(d => d.ToModel()).ToList() ?? new List<Pedido>();
    }

    // Pedido con detalle (GET /api/pedidos/{id}): además de la cabecera, trae las
    // líneas y el NIT del cliente (que no se expone en el listado).
    public static Pedido ToModel(this PedidoDetalleDto dto)
    {
        var pedido = ((PedidoDto)dto).ToModel();
        pedido.NitCliente = dto.ClienteNit;
        pedido.DetallesPedidos = dto.Items?.Select(i => i.ToModel()).ToList() ?? new List<DetallePedido>();

        // El DTO trae la razón social aplanada y el NIT por separado.
        if (pedido.Cliente == null && !string.IsNullOrWhiteSpace(dto.ClienteNit))
        {
            pedido.Cliente = new Cliente
            {
                IdCliente = dto.IdCliente ?? 0,
                Nit = dto.ClienteNit
            };
        }
        else if (pedido.Cliente != null)
        {
            pedido.Cliente.Nit = dto.ClienteNit;
        }

        return pedido;
    }

    // DetallePedido (línea de un pedido)
    public static DetallePedido ToModel(this PedidoItemDto dto)
    {
        return new DetallePedido
        {
            IdDetallePedido = dto.IdDetalle,
            IdPedido = 0, // el item no trae el id de la cabecera
            IdProducto = dto.IdProducto,
            NombreProducto = dto.ProductoNombre,
            Cantidad = dto.Cantidad,
            PrecioUnitario = dto.PrecioUnitario,
            Subtotal = dto.Subtotal,
            Observacion = dto.Observacion,
            Producto = new Producto { IdProducto = dto.IdProducto, Nombre = dto.ProductoNombre ?? string.Empty }
        };
    }

    public static List<DetallePedido> ToModel(this IEnumerable<PedidoItemDto>? dtos)
    {
        return dtos?.Select(d => d.ToModel()).ToList() ?? new List<DetallePedido>();
    }

    // Cola de cocina (GET /api/pedidos/cola): la API devuelve el detalle ya
    // resumido (nombre del producto, sin precios), suficiente para la pantalla.
    public static Pedido ToModel(this ColaPedidoDto dto)
    {
        return new Pedido
        {
            IdPedido = dto.IdPedido,
            TipoPedido = dto.TipoPedido,
            Estado = dto.Estado,
            EstadoPago = dto.EstadoPago,
            FechaCreacion = dto.FechaCreacion,
            NombrePedido = dto.NombrePedido,
            NumeroMesa = dto.MesaNumero,
            NombreCajero = dto.NombreCajero,
            Mesa = dto.MesaNumero.HasValue ? new Mesa { Numero = dto.MesaNumero.Value } : null,
            Empleado = new Empleado { Nombre = dto.NombreCajero ?? string.Empty },
            DetallesPedidos = dto.Detalle?.Select(d => d.ToModel()).ToList() ?? new List<DetallePedido>()
        };
    }

    public static List<Pedido> ToModel(this IEnumerable<ColaPedidoDto>? dtos)
    {
        return dtos?.Select(d => d.ToModel()).ToList() ?? new List<Pedido>();
    }

    public static DetallePedido ToModel(this ColaDetalleDto dto)
    {
        return new DetallePedido
        {
            Cantidad = dto.Cantidad,
            NombreProducto = dto.Nombre,
            Observacion = dto.Observacion,
            Producto = new Producto { Nombre = dto.Nombre ?? string.Empty }
        };
    }

    // DestinoInsumo
    public static DestinoInsumo ToModel(this DestinoInsumoDto dto)
    {
        return new DestinoInsumo
        {
            IdDestino = dto.IdDestino,
            Nombre = dto.Nombre
        };
    }

    public static List<DestinoInsumo> ToModel(this IEnumerable<DestinoInsumoDto>? dtos)
    {
        return dtos?.Select(d => d.ToModel()).ToList() ?? new List<DestinoInsumo>();
    }

    // Empleado
    public static Empleado ToModel(this EmpleadoDto dto)
    {
        return new Empleado
        {
            IdEmpleado = dto.IdEmpleado,
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Usuario = dto.Usuario,
            Telefono = dto.Telefono,
            FechaContratacion = dto.FechaContratacion,
            Estado = dto.Estado,
            IdRol = dto.IdRol,
            RolNombre = dto.RolNombre,
            Rol = new Rol
            {
                IdRol = dto.IdRol,
                Nombre = dto.RolNombre
            }
        };
    }

    public static List<Empleado> ToModel(this IEnumerable<EmpleadoDto>? dtos)
    {
        return dtos?.Select(d => d.ToModel()).ToList() ?? new List<Empleado>();
    }
}
