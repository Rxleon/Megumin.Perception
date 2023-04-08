﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Megumin.GameFramework.Perception
{
    /// <summary>
    /// Mask这里设置了，HearingSensor，SightSensor就不用设置了
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class Perception<T> : MonoBehaviour
        where T : class
    {
        public Enableable<GameObjectFilter> OverrideFilter;

        [ProtectedInInspector]
        public List<Sensor> Sensor;

        public void Awake()
        {
            var sensor2 = gameObject.GetComponentsInChildren<Sensor>(true);
            foreach (var sensor in sensor2)
            {
                if (Sensor.Contains(sensor))
                {

                }
                else
                {
                    Sensor.Add(sensor);
                }
            }
        }

        private void Reset()
        {
            if (Sensor != null)
            {
                Sensor.Clear();
                gameObject.GetComponentsInChildren(true, Sensor);
            }
        }

        [ReadOnlyInInspector]
        public List<T> InSensor = new List<T>();

        /// <summary>
        /// 更新间隔
        /// </summary>
        [Space]
        [Range(0, 5)]
        public float checkDelta = 0.5f;
        protected float nextCheckStamp;

        protected HashSet<Collider> inSensorColliders { get; } = new();
        protected HashSet<T> tempInSensor { get; } = new();

        private void Update()
        {
            if (Time.time < nextCheckStamp)
            {
                return;
            }
            nextCheckStamp = Time.time + checkDelta;
            CheckSensor();
        }

        protected virtual void CheckSensor()
        {
            inSensorColliders.Clear();

            if (Sensor != null)
            {
                GameObjectFilter filter = null;
                if (OverrideFilter.Enabled)
                {
                    filter = OverrideFilter.Value;
                }

                foreach (var item in Sensor)
                {
                    if (item != null)
                    {
                        item.TryGetInSensor(inSensorColliders, filter);
                    }
                }
            }

            tempInSensor.Clear();
            foreach (Collider c in inSensorColliders)
            {
                var tV = c.GetComponentInParent<T>();
                if (tV != null)
                {
                    tempInSensor.Add(tV);
                }
            }

            foreach (var item in InSensor)
            {
                if (tempInSensor.Contains(item))
                {

                }
                else
                {
                    //失去感知
                    OnLostTarget(item);
                }
            }

            foreach (var item in tempInSensor)
            {
                if (InSensor.Contains(item))
                {

                }
                else
                {
                    //新感知
                    OnFindTarget(item);
                }
            }

            InSensor.Clear();
            InSensor.AddRange(tempInSensor);
        }

        [ReadOnlyInInspector]
        public T AutoTarget;

        public virtual void OnFindTarget(T target)
        {
            //Debug.Log($"感知模块 发现新目标");
            if (AutoTarget == null)
            {
                AutoTarget = target;
            }
        }

        public virtual void OnLostTarget(T target)
        {
            //Debug.Log($"感知模块 失去目标");
            if (target == AutoTarget)
            {
                AutoTarget = InSensor.FirstOrDefault();
            }
        }

        [Header("Debug")]
        [Space]
        public bool DebugSolid = false;
        public Color DebugColor = Color.red;
        [Range(0, 10)]
        public float DebugLineThickness = 2;
    }
}


#if UNITY_EDITOR

namespace Megumin.GameFramework.Perception
{
    using UnityEditor;
    partial class Perception<T>
    {
        public void OnDrawGizmosSelected()
        {
            if (AutoTarget is MonoBehaviour behaviour)
            {
                Handles.color = DebugColor;
                Handles.DrawLine(transform.position, behaviour.transform.position, DebugLineThickness);
            }
        }
    }
}

#endif