namespace RaceDay.API.Models
{
    /// <summary>
    /// Join table for the many-to-many relationship between Events and Categories.
    /// </summary>
    public class EventCategory
    {
        // Composite Primary Key (EventId + CategoryId) will be configured in DbContext
        public int EventId { get; set; }
        public Event? Event { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
