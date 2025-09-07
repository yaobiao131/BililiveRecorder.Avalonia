using System;
using BililiveRecorder.Common;
using BililiveRecorder.Common.Config;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BililiveRecorder.Core.Recording;

internal class RecordTaskFactory : IRecordTaskFactory
{
    private readonly ILogger logger;
    private readonly IServiceProvider serviceProvider;
    private readonly ObjectFactory factoryRawData;
    private readonly ObjectFactory factoryStandard;

    public RecordTaskFactory(ILogger logger, IServiceProvider serviceProvider)
    {
        this.logger = logger.ForContext<RecordTaskFactory>() ?? throw new ArgumentNullException(nameof(logger));
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        factoryRawData = ActivatorUtilities.CreateFactory(typeof(RawDataRecordTask), [typeof(IRoom)]);
        factoryStandard = ActivatorUtilities.CreateFactory(typeof(StandardRecordTask), [typeof(IRoom)]);
    }

    public IRecordTask CreateRecordTask(IRoom room, RecordMode? recordModeOverride = null)
    {
        var recordMode = room.RoomConfig.RecordMode;
        if (recordModeOverride.HasValue)
        {
            recordMode = recordModeOverride.Value;
        }

        logger.Debug("Create record task with mode {RecordMode} for room {RoomId}, override: {Override}", recordMode, room.RoomConfig.RoomId, recordModeOverride);
        return recordMode switch
        {
            RecordMode.RawData => (IRecordTask)factoryRawData(serviceProvider, [room]),
            _ => (IRecordTask)factoryStandard(serviceProvider, [room])
        };
    }
}
