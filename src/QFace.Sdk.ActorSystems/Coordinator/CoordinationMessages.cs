namespace QFace.Sdk.ActorSystems.Coordinator;

/// <summary>
    /// Contains message types used in the coordinator pattern for actor communications.
    /// These standardized messages facilitate communication between coordinators and worker actors.
    /// </summary>
    public static class CoordinationMessages
    {
        /// <summary>
        /// Message sent to initialize a coordinator actor.
        /// This is typically the first message sent to a coordinator after it is created.
        /// </summary>
        public class Initialize
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Initialize"/> class.
            /// </summary>
            public Initialize() { }
        }

        /// <summary>
        /// Message for distributing work items to worker actors.
        /// The coordinator uses this message to send work to appropriate workers.
        /// </summary>
        public class DistributeWork
        {
            /// <summary>
            /// Gets the work item to be processed.
            /// This can be any object that represents the work to be done.
            /// </summary>
            public object WorkItem { get; }
            
            /// <summary>
            /// Gets or sets the actor reference to send the result to.
            /// If null, the result will be sent to the original sender.
            /// </summary>
            public IActorRef? RespondTo { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="DistributeWork"/> class.
            /// </summary>
            /// <param name="workItem">The work item to be processed</param>
            public DistributeWork(object workItem)
            {
                WorkItem = workItem;
            }
        }

        /// <summary>
        /// Message sent by workers when work is completed.
        /// Contains the result of the processing and a unique identifier for the work.
        /// </summary>
        public class WorkCompleted
        {
            /// <summary>
            /// Gets the result of the processed work.
            /// </summary>
            public object Result { get; }
            
            /// <summary>
            /// Gets the unique identifier for the work item.
            /// Used to correlate work items with their results.
            /// </summary>
            public Guid WorkId { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="WorkCompleted"/> class.
            /// </summary>
            /// <param name="result">The result of the processed work</param>
            /// <param name="workId">The unique identifier for the work item</param>
            public WorkCompleted(object result, Guid workId)
            {
                Result = result;
                WorkId = workId;
            }
        }

        /// <summary>
        /// Message used to trigger health checks for worker actors.
        /// Sent periodically to ensure workers are responsive.
        /// </summary>
        public class CheckHealth { }

        /// <summary>
        /// Simple message used for health checks.
        /// Sent to workers to check if they are responsive.
        /// </summary>
        public class Ping { }

        /// <summary>
        /// Response message for health checks.
        /// Sent by workers in response to a Ping message.
        /// </summary>
        public class Pong { }
    }