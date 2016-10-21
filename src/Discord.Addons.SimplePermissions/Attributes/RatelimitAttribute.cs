﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace Discord.Addons.SimplePermissions
{
    /// <summary>
    /// Sets how often a user is allowed to use this command.
    /// </summary>
    /// <remarks>This is backed by an in-memory collection
    /// and will not persist with restarts.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RatelimitAttribute : PreconditionAttribute
    {
        private uint InvokeLimit { get; }
        private TimeSpan InvokeLimitPeriod { get; }
        private Dictionary<ulong, CommandTimeout> InvokeTracker { get; } = new Dictionary<ulong, CommandTimeout>();

        /// <summary>
        /// Sets how often a user is allowed to use this command.
        /// </summary>
        /// <param name="times">The number of times a user may use the command within a certain period.</param>
        /// <param name="period">The amount of time since first invoke a user has until the limit is lifted.</param>
        /// <param name="measure">The scale in which the <paramref name="period"/> parameter should be measured.</param>
        public RatelimitAttribute(uint times, double period, Measure measure)
        {
            InvokeLimit = times;

            //TODO: C# 7 candidate switch expression
            switch (measure)
            {
                case Measure.Days: InvokeLimitPeriod = TimeSpan.FromDays(period);
                    break;
                case Measure.Hours: InvokeLimitPeriod = TimeSpan.FromHours(period);
                    break;
                case Measure.Minutes: InvokeLimitPeriod = TimeSpan.FromMinutes(period);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public override Task<PreconditionResult> CheckPermissions(CommandContext context, CommandInfo command, IDependencyMap map)
        {
            var now = DateTime.UtcNow;
            CommandTimeout timeout;

            if (!InvokeTracker.TryGetValue(context.User.Id, out timeout) ||
                ((now - timeout.FirstInvoke) > InvokeLimitPeriod))
            {
                timeout = new CommandTimeout(now);
            }

            timeout.TimesInvoked++;

            if (timeout.TimesInvoked <= InvokeLimit)
            {
                InvokeTracker[context.User.Id] = timeout;
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError("You are currently in Timeout."));
            }
        }

        private class CommandTimeout
        {
            public uint TimesInvoked { get; set; }
            public DateTime FirstInvoke { get; }

            public CommandTimeout(DateTime timeStarted)
            {
                FirstInvoke = timeStarted;
            }
        }
    }

    /// <summary>
    /// Sets the scale of the period parameter.
    /// </summary>
    public enum Measure
    {
        /// <summary>
        /// Period is measured in days.
        /// </summary>
        Days,

        /// <summary>
        /// Period is measured in hours.
        /// </summary>
        Hours,

        /// <summary>
        /// Period is measured in minutes.
        /// </summary>
        Minutes
    }
}
