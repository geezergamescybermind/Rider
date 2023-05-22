using UnityEngine;
using System;

public class SetSpeed : MonoBehaviour
{
	Arduino arduino;
	public AnimationCurve curve;
	private Rigidbody rb;

	public float estimatedTopSpeedKMH = 50f;
	public float kmhHigh;
	public float currentKMHTarget = 0f;
	public float currentSpeedNormalized;
	int angle;
	int lastAngle;
	float timerStored;

	int TOTAL_SECONDS_TO_MEASSURE_OVER = 3;
	int TICKS_PER_SECOND = 100;
	int TOTAL_TICKS;
	float TICK_TIME;
	int[] lastSecondTicksData;
	int currentTickIndex = 0;
	int revolutionCounter = 0;
	int oldRevolutionCounter = 0;
	float tickTimer = 0;
	int tickAngle;
	int totalRevolutionsAngle;

	bool initialReadDone = false;

	private bool[] bits = new bool[32];


	void Start()
	{
		arduino = GetComponent<Arduino>();
		rb = GetComponent<Rigidbody>();

		TOTAL_TICKS = TICKS_PER_SECOND * TOTAL_SECONDS_TO_MEASSURE_OVER;
		TICK_TIME = 1f / TICKS_PER_SECOND;
		lastSecondTicksData = new int[TOTAL_TICKS];
	}


	void Update()
	{
		timerStored += Time.deltaTime;

		angle = arduino.currentRotation;
		revolutionCounter = arduino.revolutionCounter;

		if (initialReadDone == false)
		{
			initialReadDone = true;
			lastAngle = angle;
			oldRevolutionCounter = revolutionCounter;
		}

		UpdateSpeed();

		if (currentKMHTarget > kmhHigh) kmhHigh = currentKMHTarget;
	}

	void FixedUpdate()
	{
		rb.AddForce(new Vector3(0f, 0f, curve.Evaluate(currentSpeedNormalized)) * 100f);
	}

	private float KilometersPerHourFunction(float millisecondsForOneRevolution)
	{
		float revolutionsPerHour = 3600000 / millisecondsForOneRevolution;
		float metersPerHour = revolutionsPerHour * 5; //1 revolution = 5 meters
		float kilometersPerHour = metersPerHour / 1000f;
		return kilometersPerHour;
	}

	private void UpdateSpeed()
	{
		timerStored += Time.deltaTime;

		if (tickTimer < timerStored)
		{
			tickTimer = timerStored + TICK_TIME;

			tickAngle = 0;
			if (revolutionCounter > oldRevolutionCounter)
			{
				tickAngle = (360 - lastAngle) + angle;
				oldRevolutionCounter = revolutionCounter;
			}
			else if (angle > lastAngle)
			{
				tickAngle = angle - lastAngle;
			}
			++currentTickIndex;
			if (currentTickIndex == TOTAL_TICKS) currentTickIndex = 0;
			lastSecondTicksData[currentTickIndex] = tickAngle;
			lastAngle = angle;
			totalRevolutionsAngle = 0;
			for (int t = 0; t < TOTAL_TICKS; ++t)
			{
				totalRevolutionsAngle = totalRevolutionsAngle + lastSecondTicksData[t];
			}

			//currentKMHTarget = 0f;
			if (totalRevolutionsAngle > 0)
			{
				currentKMHTarget = KilometersPerHourFunction((1000f * TOTAL_SECONDS_TO_MEASSURE_OVER) / (totalRevolutionsAngle / 360f)); //FIX IT! Some divide by zero FIX IT!
			}
			currentSpeedNormalized = Mathf.Clamp01(currentKMHTarget / estimatedTopSpeedKMH);
		}
	}
}