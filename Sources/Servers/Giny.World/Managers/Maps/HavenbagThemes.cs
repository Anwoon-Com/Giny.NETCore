using System.Collections.Generic;

namespace Giny.World.Managers.Maps
{
    /// <summary>
    /// Official haven bag themes (HavenbagThemes.d2o) : theme id -> interior map id.
    /// </summary>
    public static class HavenbagThemes
    {
        public static readonly Dictionary<byte, int> Maps = new Dictionary<byte, int>()
        {
            {1,162791424},{2,162793472},{3,162795520},{4,162791426},{5,162791428},{6,162795522},
            {7,162795524},{9,162793474},{10,162791430},{11,162793478},{12,162795526},{14,162791432},
            {15,162793480},{16,162795528},{17,162791434},{18,162791436},{19,162793484},{20,162795532},
            {21,162791438},{23,162793482},{24,162795530},{25,162793486},{26,162791440},{27,162795534},
            {28,162793488},{29,162795536},{30,162791442},{31,162793490},{32,162795538},{33,162791444},
            {34,162793492},{51,162791448},{52,162793496},{53,162795544},{54,162791450},{55,162793498},
            {56,162795546},{57,162791452},{58,162793500},
        };

        public static bool IsHavenbagMap(long mapId)
        {
            return Maps.ContainsValue((int)mapId);
        }
    }
}
