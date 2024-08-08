using System.Collections.Generic;
using UnityEngine;

public class ControlBuffer {
    private Dictionary<string, int> _inputs = new Dictionary<string, int>();
    private int _size;


    public void Init(int size) {
        _size = size;
        _inputs.Clear();
    }

    public void Update(FrameData frame) {
        Dictionary<string, int> copy = new Dictionary<string, int>(_inputs);

        foreach (KeyValuePair<string, int> pair in _inputs)
            if (pair.Value > 0)
                copy[pair.Key] = pair.Value - 1;

        _inputs = copy;

        string[] inputs = frame.inputs.Split(';');

        foreach (string input in inputs) {
            if (_inputs.ContainsKey(input))
                _inputs[input] = _size;
            else
                _inputs.Add(input, _size);
        }
    }

    public bool GetBufferedInput(KeyCode input) {
        string key = input.ToString();

        if (_inputs.ContainsKey(key))
            if (_inputs[key] > 0) {
                _inputs[key] = 0;
                return true;
            }

        return false;
    }
}
