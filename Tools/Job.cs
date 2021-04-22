using System;
using System.Threading.Tasks;

namespace Apprentice.Tools {

    public enum JobStatus {
        Unfinished,
        Success,
        Failure
    }

    public class JobBase {

        public JobStatus Status { get; protected set; }
        public bool Success => Status == JobStatus.Success;
        private TaskCompletionSource<bool> finish;

        /// <summary>Create an unfinished job</summary>
        public JobBase() {
            Status = JobStatus.Unfinished;
        }

        /// <summary>Create a job with a specific status</summary>
        public JobBase(JobStatus status) {
            Status = status;
        }

        /// <summary>Fail the job with a default result</summary>
        public void Fail() {
            if (Status != JobStatus.Unfinished)
                throw new Exception();
            Status = JobStatus.Failure;
            finish.SetResult(false);
        }

        /// <summary>Complete the job with a default result</summary>
        public void Complete() {
            if (Status != JobStatus.Unfinished)
                throw new Exception();
            Status = JobStatus.Success;
            finish.SetResult(true);
        }

        /// <summary>Wait for the job to finish (success or failure)</summary>
        public async Task<bool> WaitForFinish() {
            if (finish == null)
                finish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            return await finish.Task;
        }

        public override string ToString() => $"{{ Job: {Status} }}";
    }

    public class Job : JobBase {
        public Job() : base() { }
        public Job(JobStatus status) : base(status) { }
        /// <summary>Create a failed job with default result</summary>
        public static Job Failed() => new Job(JobStatus.Failure);
        /// <summary>Create a completed job with default result</summary>
        public static Job Completed() => new Job(JobStatus.Success);
        /// <summary>Create a completed job with some result</summary>
        public static Job<T> Completed<T>(T result) => new Job<T>(result);
        /// <summary>Create a failed job with some result</summary>
        public static Job<T> Failed<T>(T result) => new Job<T>(JobStatus.Failure, result);
    }

    public class Job<T> : JobBase {
        /// <summary>Result of the job</summary>
        public T Result { get; private set; }

        /// <summary>Create an unfinished job with a default result</summary>
        public Job() : base() { }

        /// <summary>Create a job with a specific status and result</summary>
        public Job(JobStatus status, T result = default) : base(status) {
            Result = result;
        }

        /// <summary>Create a completed job with a specific result</summary>
        public Job(T result) : base(JobStatus.Success) {
            Result = result;
        }

        /// <summary>Fail the job with some result</summary>
        public void Fail(T result) {
            Result = result;
            Fail();
        }

        /// <summary>Complete the job with some value</summary>
        public void Complete(T result) {
            Result = result;
            Complete();
        }

        public static implicit operator Job<T>(Job job) => new Job<T>(job.Status);
        public override string ToString() => $"{{ Job: {Status}, {Result} }}";
    }
}
