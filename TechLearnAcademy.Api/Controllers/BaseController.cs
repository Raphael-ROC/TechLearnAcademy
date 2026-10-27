using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TechLearnAcademy.Api.Modelos;

namespace TechLearnAcademy.Api.Controllers;

public class BaseController : ControllerBase
{
    protected Usuario Usuario => new(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), User.FindFirstValue(ClaimTypes.Name)!, User.FindFirstValue(ClaimTypes.Email)!, User.FindFirstValue(ClaimTypes.Role)!);
    protected IActionResult Mensagem(string texto) => Ok(new { mensagem = texto });
}
