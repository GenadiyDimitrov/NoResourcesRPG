using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoResourcesRPG.Shared.DTOs;
public class PlayerActionDto
{
    public string PlayerId { get; set; }
    public string Action { get; set; } // "Move", "Collect"
    public int X { get; set; }
    public int Y { get; set; }
}
