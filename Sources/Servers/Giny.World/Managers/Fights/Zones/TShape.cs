using Giny.Protocol.Custom.Enums;
using Giny.World.Managers.Fights.Cast;
using Giny.World.Managers.Maps;
using Giny.World.Records.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Managers.Fights.Zones
{
    /// <summary>
    /// T shaped zone : a perpendicular bar through the target cell plus a
    /// stem running from the target back toward the caster.
    /// </summary>
    public class TShape : Zone
    {
        public TShape(byte radius)
        {
            this.Radius = radius;
        }

        public override CellRecord[] GetCells(CellRecord centerCell, CellRecord casterCell, MapRecord map)
        {
            var bar = new Cross(0, Radius)
            {
                OnlyPerpendicular = true,
            };
            bar.Initialize(ZoneEnum.T, Direction);

            List<CellRecord> cells = bar.GetCells(centerCell, casterCell, map).ToList();

            MapPoint point = centerCell.Point;
            DirectionsEnum stem = Direction.GetOpposedDirection();

            for (int i = 0; i < Radius; i++)
            {
                point = point.GetCellInDirection(stem, 1);

                if (point == null)
                {
                    break;
                }

                CellRecord cell = map.GetCell(point.CellId);

                if (cell == null)
                {
                    break;
                }

                if (!cells.Contains(cell))
                {
                    cells.Add(cell);
                }
            }

            return cells.ToArray();
        }
    }
}
