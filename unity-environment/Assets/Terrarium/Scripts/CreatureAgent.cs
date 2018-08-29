using UnityEngine;
using MLAgents;

public enum CreatureType
{
	Herbivore,
	Carnivore
}

public class CreatureAgent : Agent {
	[Header("Creature Type")]
	public CreatureType CreatureType;
	[Header("Creature Points (100 Max)")]
	public float MaxEnergy;
	public float MatureSize;
	public float GrowthRate;
	public float EatingSpeed;
	public float MaxSpeed;
	public float AttackDamage;
	public float DefendDamage;
	public float Eyesight;

	[Header("Monitoring")]
	public float Energy;
	public float Size;
	public float Age;
	public string currentAction;

	[Header("Child")]
	public GameObject ChildSpawn;

	[Header("Species Parameters")]
	public float AgeRate = 0.001f;
	private GameObject Environment;
	private Rigidbody agentRB;
	private float nextAction;
	private bool died;
	private RayPerception rayPer;
	private int count;
	private Vector2 bounds;

	private void Awake()
	{
		AgentReset();
	}

	public override void AgentReset()
	{
		Size = 1;
		Energy = 1;
		Age = 0;
		bounds = GetEnvironmentBounds();
		var x = Random.Range(-bounds.x, bounds.x);
		var z = Random.Range(-bounds.y, bounds.y);
		transform.position = new Vector3(x, 1, z);
		TransformSize();
		InitializeAgent();
	}

	public override void AgentOnDone()
	{

	}

	public override void InitializeAgent()
	{
		base.InitializeAgent();
		rayPer = GetComponent<RayPerception>();
		agentRB = GetComponent<Rigidbody>();
		currentAction = "Idle";
	}

	public override void CollectObservations()
	{
		float rayDistance = Eyesight;
		float[] rayAngles = {20f, 90f, 160f, 45f, 135f, 70f, 110f};
		string[] detectableObjects = {"plant", "herbivore", "carnivore"};
		AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
		Vector3 localVelocity = transform.InverseTransformDirection(agentRB.velocity);
		AddVectorObs(localVelocity.x);
		AddVectorObs(localVelocity.z);
		AddVectorObs(Energy);
		AddVectorObs(Size);
		AddVectorObs(Age);
		AddVectorObs(Float(CanEat));
		AddVectorObs(Float(CanReproduce));
	}

	private float Float(bool val)
	{
		if(val) return 1.0f;
		else return 0.0f;
	}

	public override void AgentAction(float[] vectorAction, string textAction)
	{
		// Action Space 7 float
		// 0 = Move
		// 1 = Eat
		// 2 = Reproduce
		// 3 = Attack
		// 4 = Defend
		// 5 = move orders
		// 6 = rotation
		if(vectorAction[0] > .5)
		{
			MoveAgent(vectorAction);
		}
		else if(vectorAction[1] > .5)
		{
			Eat();
		}
		else if(vectorAction[2] > .5)
		{
			Reproduce();
		}
		else if(vectorAction[3] > .5)
		{
			Attack();
		}
		else if(vectorAction[4] > .5)
		{
		    Defend();
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(OutOfBounds)
		{
			AddReward(-1f);
			Done();
			return;
		}
		if(Buried)
		{
			Done();
		}
		if(Dead) return;
		if(CanGrow) Grow();
		if(CanReproduce) Reproduce();
		Age += AgeRate;
		MonitorLog();
	}

	public void FixedUpdate()
	{
		if(Time.timeSinceLevelLoad > nextAction)
		{
			currentAction = "Deciding";
			RequestDecision();
		}
	}

	public void MonitorLog()
	{
		Monitor.Log("Action", currentAction, transform);
		Monitor.Log("Size", Size/MatureSize, transform);
		Monitor.Log("Energy", Energy/MaxEnergy, transform);
		Monitor.Log("Age", Age/MatureSize, transform);
	}

	public bool OutOfBounds
	{
		get
		{
			if(transform.position.y < 0) return true;

			if(transform.position.x > bounds.x || transform.position.x < -bounds.x || 
			   transform.position.y > bounds.y || transform.position.y < -bounds.y)
		    {
				return true;
			}
			else return false;
		}
	}

	void TransformSize()
	{
		transform.localScale = Vector3.one * Mathf.Pow(Size, 1/2);
	}

	bool CanGrow
	{
		get
		{
			return Energy > ((MaxEnergy / 2) + 1);
		}
	}

	bool CanEat
	{
		get
		{
			if(CreatureType == CreatureType.Herbivore)
			{
				if(FirstAdjacent("plant") != null) return true;
			}
			return false;
		}
	}

	private GameObject FirstAdjacent(string tag)
	{
		var colliders = Physics.OverlapSphere(transform.position, 1.2f*Size);
		foreach(var collider in colliders)
		{
			if(collider.gameObject.tag == tag)
			{
				return collider.gameObject;
			}
		}

		return null;
	}

	bool CanReproduce
	{
		get 
		{
			if(Size >= MatureSize && CanGrow) return true;
			else return false;
		}
	}

	bool Dead 
	{
		get
		{
			if(died) return true;
			if(Age > MatureSize)
			{
				currentAction = "Dead";
				died = true;
				Energy = Size; // Creature size is converted to energy
				AddReward(.2f);
				Done();
				return true;
			}
			return false;
		}
	}
	
	bool Buried
	{
		get
		{
			Energy -= AgeRate;
			return Energy < 0;
		}
	}

	void Grow()
	{
		if(Size > MatureSize) return;
		Energy = Energy / 2;
		Size += GrowthRate * Random.value;
		nextAction = Time.timeSinceLevelLoad + (25/MaxSpeed);
		currentAction = "Growing";
		TransformSize();
	}

	void Reproduce()
	{
		if(CanReproduce)
		{
			var vec = Random.insideUnitCircle * 5;
			var go = Instantiate(ChildSpawn, new Vector3(vec.x, 0, vec.y), Quaternion.identity, Environment.transform);
			go.name = go.name + (count++).ToString();
			var ca = go.GetComponent<CreatureAgent>();
			ca.AgentReset();
			Energy = Energy / 2;
			AddReward(.2f);
			currentAction = "Reproducing";
			nextAction = Time.timeSinceLevelLoad + (25/MaxSpeed);
		}
	}

	public void Eat()
	{
		if(CreatureType == CreatureType.Herbivore)
		{
			var adj = FirstAdjacent("plant");
		    if(adj != null)
			{
				var creature = adj.GetComponent<Plant>();
				var consume = Mathf.Min(creature.Energy, 5);
				creature.Energy -= consume;
				if(creature.Energy < .1f) Destroy(adj);
				Energy += consume;
				AddReward(.1f);
				nextAction = Time.timeSinceLevelLoad + (25/EatingSpeed);
				currentAction = "Eating";
			}
		}
	}

	public void MoveAgent(float[] act)
	{
		Vector3 rotateDir = Vector3.zero;
		rotateDir = transform.up * Mathf.Clamp(act[6], -1f, 1f);
		if(act[5] > 0.5f)
		{
			transform.position = transform.position + transform.forward;
		}
		Energy -= .01f;
		transform.Rotate(rotateDir, Time.fixedDeltaTime * MaxSpeed);
		currentAction = "Moving";
		nextAction = Time.timeSinceLevelLoad + (25/MaxSpeed);
	}

	private Vector2 GetEnvironmentBounds()
	{
		Environment = transform.parent.gameObject;
		var xs = Environment.transform.localScale.x;
		var zs = Environment.transform.localScale.z;
		return new Vector2(xs, zs) * 10;
	}
	void Defend()
	{
		currentAction = "Defend";
		nextAction = Time.timeSinceLevelLoad + (25/MaxSpeed);
	}
	void Attack()
	{
		float damage = 0f;
		currentAction = "Attack";
		nextAction = Time.timeSinceLevelLoad + (25 / MaxSpeed);
		var vic = FirstAdjacent("herbivore").GetComponent<CreatureAgent>();
		if(vic != null)
		{
			if(vic.currentAction == "Defend")
			{
				damage = ((AttackDamage * Size) - (vic.DefendDamage * vic.Size)) / (Size * vic.Size);
			}
			else
			{
				damage = ((AttackDamage * Size) - (1 * vic.Size)) / (Size * vic.Size);
			}
		}
		else
		{
		    vic = FirstAdjacent("carnivore").GetComponent<CreatureAgent>();
		    if(vic != null)
		    {
			    if(vic.currentAction == "Attack")
			    {
				    damage = ((AttackDamage * Size) - (vic.AttackDamage * vic.Size)) / (Size * vic.Size);
			    }
			    else
			    {
				    damage = ((AttackDamage * Size) - (vic.DefendDamage * vic.Size)) / (Size * vic.Size);
				}
			}
		}

		if(damage > 0)
		{
			vic.Energy -= damage;
			if(vic.Energy < 0)
			{
				AddReward(.25f);
			}
		}
		else if(damage < 0)
		{
			Energy -= damage;
		}
		Energy -= .1f;
	}
}
