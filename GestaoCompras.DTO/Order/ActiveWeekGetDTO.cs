namespace GestaoCompras.DTO.Order
{
    public class ActiveWeekGetDTO
    {
        public Guid Id { get; set; }
        public string OptionText { get; set; }
        public DateTime InitialDate { get; set; }
        public DateTime EndDate { get; set; }

        public ActiveWeekGetDTO()
        {

        }
    }
}
