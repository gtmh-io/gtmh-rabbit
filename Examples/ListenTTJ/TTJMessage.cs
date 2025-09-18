using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ProtoBuf;
using System.Text;
using System.Threading.Tasks;

namespace Tofye.Racing
{

	/// <summary>
	/// Jump Estimation Type
	/// 
	/// TODO -
	/// * can remove ScheuldedTime? Going forward we will publish TTJ by JET and JET.Default will always be published
	/// * can remove stuff related to multi?
	/// * can remove stuff related to status? Cannot ensure consistency with multiple source for status
	/// 
	/// </summary>
	public enum JET { Unset = -1, Default = 0, Manual = 1, AlgoVideo_0 = 3, UnitTest_0 = 255, UnitTest_1 = 254 };
  [DataContract][ProtoContract]
  public class TTJMessage
  {
    [DataMember(Order = 10)][ProtoMember(10)]
    public readonly DateTime ReferenceTime;
    [DataMember(Order = 20)][ProtoMember(20)]
    public readonly DateTime EstimatedJump;
    /// <summary> Race OR Multi </summary>
    [DataMember(Order = 30)][ProtoMember(30)]
    public readonly int Id;
    [DataMember(Order = 50)][ProtoMember(50)]
    public readonly bool IsMulti;
		/// <summary>
		/// This is the time it should jump as determined by the track and communicated and filtered via the RNS.
		/// It may or may not be the same as the estimated jump time.
		/// </summary>
		[DataMember(Order = 70)][ProtoMember(70)]
		public DateTime ScheduledTime { get; private set; }
		[DataMember(Order = 80)][ProtoMember(80)]
		public JET EstimationType { get; private set; }

    public TimeSpan EstTTJ_FromRef { get { return EstimatedJump.Subtract(ReferenceTime); } }  // TODO - this could make us miss jump. Should assume clocks are synch'd and check wrt DateTime.UtcNow
		public TimeSpan GetEstTTJ(DateTime a_UtcNow) { return EstimatedJump.Subtract(a_UtcNow); }

		public string Context { get { return $"{(IsMulti ? 'M' : 'R')}{Id}"; } }

    protected TTJMessage() { }	// PB
		[System.Diagnostics.DebuggerStepThrough]
    public TTJMessage(int id, DateTime refTime, DateTime estJump, DateTime schedTime, bool isMulti, JET a_ET)
    {
      Id = id;
      ReferenceTime = refTime;
      EstimatedJump = estJump;
			ScheduledTime = schedTime;
      IsMulti = isMulti;
			EstimationType = a_ET;
    }
		[System.Diagnostics.DebuggerStepThrough]
    public static TTJMessage Race(int id, DateTime refTime, DateTime estJump, DateTime schedTime, JET a_ET)
    {
      return new TTJMessage(id, refTime, estJump, schedTime, false, a_ET);
    }
		[System.Diagnostics.DebuggerStepThrough]
    public static TTJMessage Multi(int id, DateTime refTime, DateTime estJump, DateTime schedTime, JET a_ET)
    {
      return new TTJMessage(id, refTime, estJump, schedTime, true, a_ET);
    }

		public override string ToString()
		{
			return $"TTJ {(IsMulti ? 'M' : 'R')}{Id} Ref={ReferenceTime} Sched={ScheduledTime} Est={EstimatedJump} JET={EstimationType}";
		}

		[OnDeserialized]
		void OnDeser(StreamingContext sc)
		{
			if (ScheduledTime == default(DateTime)) ScheduledTime = EstimatedJump; // cross compat
			if (EstimationType == JET.Unset) EstimationType = JET.Default;
		}

		/// <summary>
		/// Please give me a TTJ for this race
		/// </summary>
		/*[DataContract][ProtoContract]
		public class ReqJumpEstMsg
		{
			[DataMember(Order=10)][ProtoMember(10)]
			public readonly int RaceId;
			[DataMember(Order=40)][ProtoMember(40)]
			public readonly string Source;  // free form, optional even

			public ReqJumpEstMsg(int a_RaceId, string a_Source = null)
			{
				RaceId = a_RaceId;
				Source = a_Source ?? AppContext.GetIdentity();
			}
			private ReqJumpEstMsg() { }
		}*/
  }
}
