using Microsoft.AspNetCore.Identity;

namespace TeamCashCenter.Data.Model;

public class Role : IdentityRole<Guid>
{
    public string Description { get; set; } = "Role description";

    public Role(string name, string description) : base(name)
    {
        Description = description;
    }
    
    public Role(string name) : base(name)
    {
    }
    public Role() : base()
    {
    }
}
