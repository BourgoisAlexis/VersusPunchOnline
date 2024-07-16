using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;

namespace DPhysx {
    public class DPhysxSpatialGrid {
        private int _cellSize;
        private Dictionary<Vector2, List<DPhysxShape>> _cells;

        public Dictionary<Vector2, List<DPhysxShape>> Cells => _cells;

        public DPhysxSpatialGrid(int size) {
            _cellSize = size;
            _cells = new Dictionary<Vector2, List<DPhysxShape>>();
        }

        //Convert world coordinates to cell coodinates
        private Vector2 WorldToCell(FixedPoint2 position) {
            int x = (int)math.floor(position.x.ToFloat() / _cellSize);
            int y = (int)math.floor(position.y.ToFloat() / _cellSize);

            return new Vector2(x, y);
        }

        public void AddBox(DPhysxBox box) {
            Vector2 minCell = WorldToCell(box.min);
            Vector2 maxCell = WorldToCell(box.max);

            for (int x = (int)minCell.X; x <= (int)maxCell.X; x++) {
                for (int y = (int)minCell.Y; y <= (int)maxCell.Y; y++) {
                    Vector2 cell = new Vector2(x, y);

                    if (!_cells.ContainsKey(cell))
                        _cells[cell] = new List<DPhysxShape>();

                    _cells[cell].Add(box);
                }
            }
        }

        public void RemoveBox(DPhysxBox box) {
            Vector2 minCell = WorldToCell(box.min);
            Vector2 maxCell = WorldToCell(box.max);

            for (int x = (int)minCell.X; x <= (int)maxCell.X; x++) {
                for (int y = (int)minCell.Y; y <= (int)maxCell.Y; y++) {
                    Vector2 cell = new Vector2(x, y);

                    if (_cells.ContainsKey(cell)) {
                        _cells[cell].RemoveAll(x => x.id == box.id);

                        if (_cells[cell].Count <= 0)
                            _cells.Remove(cell);
                    }
                }
            }
        }

        public List<DPhysxBox> GetBoxes(DPhysxBox box) {
            List<DPhysxBox> results = new List<DPhysxBox>();
            Vector2 minCell = WorldToCell(box.min);
            Vector2 maxCell = WorldToCell(box.max);

            for (int x = (int)minCell.X; x <= (int)maxCell.X; x++) {
                for (int y = (int)minCell.Y; y <= (int)maxCell.Y; y++) {
                    Vector2 cell = new Vector2(x, y);

                    if (_cells.ContainsKey(cell))
                        foreach (DPhysxBox candidate in _cells[cell])
                            if (!results.Contains(candidate))
                                results.Add(candidate);
                }
            }
            return results;
        }

        public void Update(DPhysxBox old, DPhysxBox newBox) {
            RemoveBox(old);
            AddBox(newBox);
        }
    }
}
