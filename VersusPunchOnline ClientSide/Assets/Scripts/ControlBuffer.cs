using System.Collections.Generic;

public class ControlBuffer : IInputUser {
    private Dictionary<string, int> _inputs = new Dictionary<string, int>();
    private int _size;


    public void Init(int size) {
        _size = size;
        _inputs.Clear();
    }

    public void ExecuteInputs(List<string> inputs) {
        Dictionary<string, int> copy = new Dictionary<string, int>(_inputs);

        foreach (KeyValuePair<string, int> pair in _inputs)
            if (pair.Value > 0)
                copy[pair.Key] = pair.Value - 1;

        _inputs = copy;

        foreach (string input in inputs) {
            if (_inputs.ContainsKey(input))
                _inputs[input] = _size;
            else
                _inputs.Add(input, _size);
        }
    }

    public bool GetBufferedInput(InputAction input) {
        string key = input.ToString();

        if (_inputs.ContainsKey(key))
            if (_inputs[key] > 0) {
                _inputs[key] = 0;
                return true;
            }

        return false;
    }

    public int GetBufferValue(InputAction input) {
        string key = input.ToString();

        if (_inputs.ContainsKey(key))
            return _inputs[key];

        return 0;
    }
}
