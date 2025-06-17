using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoCompras.DTO.Common;

public class GetWithPaginationGetDTO<T>
{
    public List<T> ObjectClass { get; set; }
    public int TotalRegs { get; set; }
    public int TotalPages { get; set; }
    public double AllToPay { get; set; }
    public double AllToPayTicket { get; set; }
    public double AllToPayStore1 { get; set; }
    public double AllToPayStore2 { get; set; }
    public double AllToPayMoney { get; set; }


    public GetWithPaginationGetDTO() { }

    public GetWithPaginationGetDTO(List<T> objectClass, int totalRegs, int totalPages, double allToPay, double allToPayTicket, double allToPayStore1, double allToPayStore2, double allToPayMoney)
    {
        ObjectClass = objectClass;
        TotalRegs = totalRegs;
        TotalPages = totalPages;
        AllToPay = allToPay;
        AllToPayTicket = allToPayTicket;
        AllToPayStore1 = allToPayStore1;
        AllToPayStore2 = allToPayStore2;
        AllToPayMoney = allToPayMoney;
    }
}
