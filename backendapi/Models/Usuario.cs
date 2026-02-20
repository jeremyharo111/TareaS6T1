using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backendapi.Models;

public class Usuario
{
    [Key]
    public int iduser { get; set; }

    [Required]
    [MaxLength(100)]
    public string nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string apellido { get; set; } = string.Empty;

    public DateTime fecha_nacimiento { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string correo { get; set; } = string.Empty;

    public bool activo { get; set; } = true;

    [Required]
    [MaxLength(50)]
    public string username { get; set; } = string.Empty;

    [Required]
    public string passwordhash { get; set; } = string.Empty;
}
