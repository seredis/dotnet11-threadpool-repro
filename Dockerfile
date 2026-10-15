# One image per grid row. Built and run on the SDK image of the runtime under test, so the runtime
# measured is exactly the one in that image (the SDK images carry the matching runtime).
#
#   APP=Repro                  the Redis-free repro (src/Repro)
#   APP=RedisRepro             StackExchange.Redis (src/RedisRepro), built from source at SER_REF
#   TFM=net10.0|net11.0        net11.0 needs a .NET 11 SDK_IMAGE
#   RUNTIME_ASYNC=off|on       compile the app (and StackExchange.Redis/RESPite) with runtime async
ARG SDK_IMAGE
FROM ${SDK_IMAGE}
ARG APP=Repro
ARG TFM=net10.0
ARG RUNTIME_ASYNC=off
ARG SER_REF=
ARG SER_MAJOR=4

WORKDIR /src
# StackExchange.Redis at an exact commit or tag; versioning (Nerdbank.GitVersioning) needs full
# history, so it is switched off below and the commit is recorded in every result instead.
RUN if [ -n "$SER_REF" ]; then \
      mkdir ser && cd ser && git init -q \
      && git fetch -q --depth 1 https://github.com/StackExchange/StackExchange.Redis.git "$SER_REF" \
      && git checkout -q FETCH_HEAD && git log -1 --format='%H %s'; \
    fi
COPY build/ build/
COPY src/ src/
RUN dotnet publish src/$APP -c Release -o /app \
      -p:Tfm=$TFM -p:RuntimeAsync=$RUNTIME_ASYNC \
      $( [ "$RUNTIME_ASYNC" = on ] || echo -p:DisableRuntimeAsync=true ) \
      -p:SerMajor=$SER_MAJOR -p:SerRoot=/src/ser -p:NBGV_GitEngine=Disabled \
      -p:CustomAfterMicrosoftCommonTargets=/src/build/inject.targets \
      -p:CustomAfterMicrosoftCommonCrossTargetingTargets=/src/build/inject.targets

WORKDIR /app
ENV APP=${APP} SER_REF=${SER_REF}
ENTRYPOINT ["sh", "-c", "exec dotnet /app/$APP.dll \"$@\"", "--"]
