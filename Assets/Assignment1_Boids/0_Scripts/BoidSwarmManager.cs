
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace BoidSystem
{
    public class BoidSwarmManager : MonoBehaviour
    {
        [Header("Rule Toggles")]
        [SerializeField] private bool _ruleOne = true;
        [SerializeField] private bool _ruleTwo = true;
        [SerializeField] private bool _ruleThree = true;
        [SerializeField] private bool _ruleFour = true;

        [Header("Boid Settings")]
        [SerializeField] private float _boidCentering = 100f;
        [SerializeField] private float _boidDistancing = 100f;
        [SerializeField] private float _boidCorrecting = 1f;
        [SerializeField] private Vector2 _boidVelocityMinMax = new(0.1f, 1f);

        [Header("Bounds Settings")]
        [SerializeField] private Vector2 _xBoundsMinMax = new(-1, 1);
        [SerializeField] private Vector2 _yBoundsMinMax = new(-1, 1);
        [SerializeField] private Vector2 _zBoundsMinMax = new(-1, 1);

        [Header("Velocity Start Settings")]
        [SerializeField] private Vector2 _xVelocityMinMax = new(-1, 1);
        [SerializeField] private Vector2 _yVelocityMinMax = new(-1, 1);
        [SerializeField] private Vector2 _zVelocityMinMax = new(-1, 1);

        [Header("Spawning Settings")]
        [SerializeField] private int _boidStartCount;
        [SerializeField] private Transform _boidParent;
        [SerializeField] private GameObject _boidPrefab;

        [Header("Spawn Sphere Settings")]
        [SerializeField] private float _sphereRadius = 1f;
        [SerializeField] private Vector3 _sphereOffsets = Vector3.zero;

        [Header("Debugging")]
        [SerializeField] private List<BoidClass> _boidList = new();


        #region Start Functions

        void Start()
        {
            SpawnBoids(); // Is == draw_boids()
            RandomizeBoidsPositions(); // Is == initialise_positions();
            RandomizeBoidsVelocities(); // Experiment to add randomness
        }

        private void SpawnBoids()
        {
            for (int i = 0; i < _boidStartCount; i++)
            {
                GameObject boidObject = Instantiate(_boidPrefab, _boidParent);
                string boidName = $"Boid {i}";
                BoidClass boidClass = new(boidObject, boidName);
                _boidList.Add(boidClass);
            }
        }

        private void RandomizeBoidsPositions()
        {
            Vector3 position;
            int amount = _boidList.Count;
            for (int i = 0; i < amount; i++)
            {
                position = CreateRandomPositionInsideSphere();
                _boidList[i].SetNewPosition(position);
            }
        }

        private void RandomizeBoidsVelocities()
        {
            Vector3 velocity;

            int amount = _boidList.Count;
            for (int i = 0; i < amount; i++)
            {
                velocity.x = Random.Range(_xVelocityMinMax.x, _xVelocityMinMax.y);
                velocity.y = Random.Range(_yVelocityMinMax.x, _yVelocityMinMax.y);
                velocity.z = Random.Range(_zVelocityMinMax.x, _zVelocityMinMax.y);

                velocity = VelocityCapper(velocity);
                _boidList[i].SetNewVelocity(velocity);
            }
        }

        private Vector3 CreateRandomPositionInsideSphere()
        {
            Vector3 position;
            position = Random.insideUnitSphere * _sphereRadius;
            position += _sphereOffsets;
            return position;
        }

        #endregion

        #region Update Functions

        void Update()
        {
            MoveAllBoidsToNewPositions(); // Is == move_all_boids_to_new_positions()
        }

        private void MoveAllBoidsToNewPositions()
        {
            Vector3 vRuleOne, vRuleTwo, vRuleThree, vRuleFour;
            BoidClass boidClass;
            Vector3 boidVelocity;
            Vector3 boidPosition;
            Vector3 velocityCapped;

            int amount = _boidList.Count;
            for (int i = 0; i < amount; i++)
            {
                boidClass = _boidList[i];
                boidVelocity = boidClass.GetBoidVelocity();
                boidPosition = boidClass.GetBoidPosition();

                vRuleOne = RuleOne(boidClass);
                vRuleTwo = RuleTwo(boidClass);
                vRuleThree = RuleThree(boidClass);
                vRuleFour = RuleFour(boidClass);

                if (_ruleOne)
                    boidVelocity += vRuleOne;

                if (_ruleTwo)
                    boidVelocity += vRuleTwo;

                if (_ruleThree)
                    boidVelocity += vRuleThree;

                if (_ruleFour)
                    boidVelocity += vRuleFour;

                // Experiment capped velocity.
                velocityCapped = VelocityCapper(boidVelocity);

                // Experiment delta time.
                boidPosition += velocityCapped * Time.deltaTime;

                boidClass.SetNewPosition(boidPosition);
                boidClass.SetNewVelocity(velocityCapped);

                // Experiment rotate boid.
                boidClass.UpdateRotation();
            }
        }

        private Vector3 RuleOne(BoidClass boidClass)
        {
            Vector3 perceivedCentre = Vector3.zero;
            BoidClass tempBoid;

            int amount = _boidList.Count;
            for (int i = 0; i < amount; i++)
            {
                tempBoid = _boidList[i];
                if (boidClass != tempBoid)
                {
                    perceivedCentre += tempBoid.GetBoidPosition();
                }
            }

            perceivedCentre /= amount - 1;
            return (perceivedCentre - boidClass.GetBoidPosition()) / _boidCentering;
        }

        private Vector3 RuleTwo(BoidClass boidClass)
        {
            BoidClass tempBoid;
            Vector3 perceivedDisplacement = Vector3.zero;
            Vector3 currentBoidPosition = boidClass.GetBoidPosition();
            Vector3 tempBoidPosition;
            float distance;

            int amount = _boidList.Count;
            for (int i = 0; i < amount; i++)
            {
                tempBoid = _boidList[i];

                if (boidClass != tempBoid)
                {
                    tempBoidPosition = tempBoid.GetBoidPosition();
                    distance = Vector3.Distance(tempBoidPosition, currentBoidPosition);

                    if (distance < _boidDistancing)
                    {
                        // Tweaked slightly to make closests boids have the most push back.
                        perceivedDisplacement -= (tempBoidPosition - currentBoidPosition) / distance;
                    }
                }
            }

            return perceivedDisplacement;
        }

        private Vector3 RuleThree(BoidClass boidClass)
        {
            BoidClass tempBoid;
            Vector3 currentVelocity = boidClass.GetBoidVelocity();
            Vector3 perceivedVelocity = Vector3.zero;

            int amount = _boidList.Count;
            for (int i = 0; i < amount; i++)
            {
                tempBoid = _boidList[i];

                if (boidClass != tempBoid)
                {
                    perceivedVelocity += tempBoid.GetBoidVelocity();
                }
            }

            perceivedVelocity /= _boidList.Count - 1;

            return (perceivedVelocity - currentVelocity) / 8;
        }

        private Vector3 RuleFour(BoidClass boidClass)
        {
            Vector3 perceivedCorrection = Vector3.zero;
            Vector3 currentBoidPosition = boidClass.GetBoidPosition();

            if (currentBoidPosition.x < _xBoundsMinMax.x)
            {
                perceivedCorrection.x = _boidCorrecting;
            }
            else if (currentBoidPosition.x > _xBoundsMinMax.y)
            {
                perceivedCorrection.x = _boidCorrecting * -1;
            }

            if (currentBoidPosition.y < _yBoundsMinMax.x)
            {
                perceivedCorrection.y = _boidCorrecting;
            }
            else if (currentBoidPosition.y > _yBoundsMinMax.y)
            {
                perceivedCorrection.y = _boidCorrecting * -1;
            }

            if (currentBoidPosition.z < _zBoundsMinMax.x)
            {
                perceivedCorrection.z = _boidCorrecting;
            }
            else if (currentBoidPosition.z > _zBoundsMinMax.y)
            {
                perceivedCorrection.z = _boidCorrecting * -1;
            }

            return perceivedCorrection;
        }

        private Vector3 VelocityCapper(Vector3 velocity)
        {
            // Experiment cap via magnitude.
            float magnitude = velocity.magnitude;
            float clamped = Mathf.Clamp(magnitude, _boidVelocityMinMax.x, _boidVelocityMinMax.y);
            return velocity.normalized * clamped;

            /*
            //Experiment per axis clamp caused issues switched to magnitude approach instead.

            velocity.x = Mathf.Clamp(velocity.x, _boidVelocityMinMax.x, _boidVelocityMinMax.y);
            velocity.y = Mathf.Clamp(velocity.y, _boidVelocityMinMax.x, _boidVelocityMinMax.y);
            velocity.z = Mathf.Clamp(velocity.z, _boidVelocityMinMax.x, _boidVelocityMinMax.y);

            return velocity;
            */
        }

        #endregion
    }

}
