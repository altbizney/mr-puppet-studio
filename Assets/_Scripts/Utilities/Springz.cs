// Copyright Thinko 2019

using UnityEngine;

public static class Springz
{
    public static float Float(float current, float target, ref float velocity, float stiffness = 100f, float damping = 10f, float maxVelocity = Mathf.Infinity)
    {
        float dampingFactor = Mathf.Max(0f, 1f - damping * Time.smoothDeltaTime);
        float acceleration = (target - current) * stiffness * Time.smoothDeltaTime;
        velocity = velocity * dampingFactor + acceleration;

        if (maxVelocity < Mathf.Infinity)
            velocity = Mathf.Clamp(velocity, -maxVelocity, maxVelocity);

        current += velocity * Time.smoothDeltaTime;

        if (Mathf.Abs(current - target) < 0.01f && Mathf.Abs(velocity) < 0.01f)
        {
            current = target;
            velocity = 0f;
        }

        return current;
    }
}