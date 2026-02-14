using UnityEngine;

namespace BoidSystem
{
    [System.Serializable]
    public class BoidClass
    {
        // Debugging
        [SerializeField] private string _name;
        [SerializeField] private GameObject _gameObject;
        [SerializeField] private Vector3 _velocity;
        [SerializeField] private Vector3 _position;

        public Vector3 GetBoidVelocity() => _velocity;

        public Vector3 GetBoidPosition() => _gameObject.transform.position;

        public void SetNewPosition(Vector3 newPosition)
        {
            _gameObject.transform.position = newPosition;
            _position = newPosition;
        }

        public void SetNewVelocity(Vector3 newVelocity)
        {
            _velocity = newVelocity;
        }

        public void UpdateRotation()
        {
            Vector3 lookPosition = _gameObject.transform.position + _velocity;
            _gameObject.transform.LookAt(lookPosition);
        }

        public BoidClass(GameObject gameObject, string name)
        {
            _gameObject = gameObject;
            _name = name;
        }
    }
}

