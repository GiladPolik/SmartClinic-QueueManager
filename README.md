# Smart Clinic Triage & Queue Management System

A multi-threaded, priority-based queue management engine designed to optimize patient flow in a medical clinic.
This system handles real-time patient registration, dynamic priority calculation (Ageing),
thread-safe resource allocation, and automated workload optimization.

## Core Architecture & Design Decisions

### 1. Data Structures: Custom Max-Heap Priority Queue
The core of the waiting list is built upon C#'s `PriorityQueue<TElement, TPriority>`. 
However, since the default implementation is a Min-Heap, I implemented a custom `IComparer` to reverse the logic into a **Max-Heap**. 
The queue evaluates priority based on a 3-tier Tie-Breaker system:
1. **Dynamic Score (Highest first):** Calculated based on medical urgency and wait time.
2. **Arrival Time (Earliest first):** Resolution down to exact `Ticks`.
3. **Age (Youngest first):** Resolves edge cases where multiple patients arrive simultaneously with identical urgency.
Note that in real time case, 2 patients probablly won't arrive at the same second, so in almost all cases, 
the arrival time will decide who came first.

### 2. Algorithmic Efficiency: $O(1)$ Ageing (Starvation Prevention)
To prevent "Starvation" (where low-urgency patients wait forever), the system implements the **Strategy Pattern** via `IPriorityStrategy`. 
Instead of recalculating the score of every waiting patient every minute (O(N)), 
the `AgeingPriorityStrategy` calculates a **static key** upon entry. 
It subtracts the arrival time  multiplied by a weight factor.
This guarantees that older patients mathematically naturally "bubble up" the Max-Heap over time, 
achieving Ageing with O(log N) enqueue/dequeue times.

### 3. Concurrency & Thread Safety
The `Manager` class acts as the thread-safe orchestrator for the queues. 
1. **Global Synchronization:** Uses `lock (m_Lock)` around critical sections, 
to ensure atomic operations when multiple medical staff (threads) request the next patient simultaneously.
2. **Fail-Fast Guard Clauses:** Patient validation occurs *outside* the lock. 
Invalid data (e.g., negative age, future arrival times) throws an exception immediately, preventing unnecessary thread blocking (Lock Contention) and maintaining data integrity.

### 4. Resource Optimization (Option A: Nurse-Doctor Fallback)
To implement the requirement of routing downgradable doctor-patients to available nurses, I utilized **Interface Segregation (ISP)**. 
The core `IClinicManager` remains pure, while `IOptimizedClinicManager` extends it with `TryOptimizeDoctorQueue()`. 
When a Nurse becomes available, the system uses the `Peek()` method on the Doctor's queue. 
If the most urgent patient is flagged as `m_IsDowngradable`, they are safely dequeued and routed to the Nurse, maximizing throughput without violating queue boundaries.

## Running the Simulation

The `Program.cs` includes a real-time event loop simulation using a dataset of 50 edge-case patients (`patients.json`). 

**To run:**
1. Ensure `patients.json` is set to **Copy to Output Directory -> Copy always** in Visual Studio.
2. Run the application. 
3. The console will demonstrate:
   - Data validation rejections.
   - Real-time optimization intercepts (highlighted in Green).
   - Starvation prevention and Tie-breaking in action.
