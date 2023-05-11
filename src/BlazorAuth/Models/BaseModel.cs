using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAuth.Models {
    public class BaseModel {
        [NotMapped] public bool FromFb { get; set; }
    }
}
