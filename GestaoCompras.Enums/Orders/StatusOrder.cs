using System.ComponentModel;

namespace GestaoCompras.Enums.Orders;

public enum StatusOrder
{
    [Description("A Pagar")]
    A_PAGAR = 0,

    [Description("Pago")]
    PAGO = 1,

    [Description("Boleto ou cheque")]
    BOLETO_CHEQUE = 2
}
