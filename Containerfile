ARG BASE_IMAGE=docker.io/library/debian:trixie-slim

FROM ${BASE_IMAGE}

LABEL org.opencontainers.image.title="SCTools-Modern Dev"
LABEL org.opencontainers.image.description="Development environment for SCTools-Modern (.NET 10, C# 14)"
LABEL org.opencontainers.image.source="https://github.com/dantte-lp/SCTools-Modern"

# Avoid interactive prompts
ENV DEBIAN_FRONTEND=noninteractive

# Install prerequisites
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        curl \
        git \
        ca-certificates \
        libicu-dev \
        libssl-dev \
    && rm -rf /var/lib/apt/lists/*

# Install .NET SDK via official install script
ARG DOTNET_CHANNEL=10.0
ARG DOTNET_QUALITY=ga

RUN curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
    && chmod +x /tmp/dotnet-install.sh \
    && /tmp/dotnet-install.sh \
        --channel "${DOTNET_CHANNEL}" \
        --quality "${DOTNET_QUALITY}" \
        --install-dir /usr/share/dotnet \
    && rm /tmp/dotnet-install.sh \
    && ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet

ENV DOTNET_ROOT=/usr/share/dotnet
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_NOLOGO=1

WORKDIR /workspace
