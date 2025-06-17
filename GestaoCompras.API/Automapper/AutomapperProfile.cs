using GestaoCompras.DTO.Access;
using GestaoCompras.DTO.Fruit;
using GestaoCompras.DTO.Order;
using GestaoCompras.DTO.Supplier;
using GestaoCompras.DTO.Users;
using GestaoCompras.DTO.Store;
using GestaoCompras.Models.Access;
using GestaoCompras.Models.Fruits;
using GestaoCompras.Models.Orders;
using GestaoCompras.Models.Stores;
using GestaoCompras.Models.Suppliers;
using GestaoCompras.Models.Users;
using System.Xml;

namespace GestaoCompras.API.Automapper;

public class AutomapperProfile : AutoMapper.Profile
{
    public AutomapperProfile()
    {
        #region CustomTypeConverters
        CreateMap<string, TimeSpan>().ConvertUsing(s => XmlConvert.ToTimeSpan(s));
        CreateMap<TimeSpan, string>().ConvertUsing(s => XmlConvert.ToString(s));
        #endregion CustomTypeConverters

        #region Access
        #region ApplicationUser
        CreateMap<ApplicationUser, ApplicationUserGetDTO>().ReverseMap();
        CreateMap<ApplicationUser, ApplicationUserPostDTO>().ReverseMap();
        #endregion ApplicationUser

        #region Fruit
        CreateMap<Fruit, FruitGetDTO>().ReverseMap();
        CreateMap<Fruit, FruitPostDTO>().ReverseMap();
        CreateMap<Fruit, FruitPutDTO>().ReverseMap();
        #endregion Fruit

        #region Order
        CreateMap<Order, OrderGetDTO>().ReverseMap();
        CreateMap<Order, OrderPostDTO>().ReverseMap();
        CreateMap<Order, OrderPutDTO>().ReverseMap();
        #endregion Order

        #region Supplier
        CreateMap<Supplier, SupplierGetDTO>().ReverseMap();
        CreateMap<Supplier, SupplierPostDTO>().ReverseMap();
        #endregion Supplier

        #region Store
        CreateMap<Store, StoreGetDTO>().ReverseMap();
        CreateMap<Store, StorePostDTO>().ReverseMap();
        #endregion Store

        #region User
        CreateMap<User, UserGetDTO>().ReverseMap();
        #endregion User
        #endregion Access

        #region User
        #region UserData
        CreateMap<UserData, UserDataGetDTO>().ReverseMap();
        #endregion UserData
        #endregion User

    }
}
