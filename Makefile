DOTNET ?= dotnet
PROJECT := src/ShapeDrawing.Option2/ShapeDrawing.Option2.csproj
TEST_PROJECT := tests/ShapeDrawing.Option2.Tests/ShapeDrawing.Option2.Tests.csproj

.PHONY: all build run test clean help

all: build

build:
	$(DOTNET) build "$(PROJECT)"

run:
	$(DOTNET) run --project "$(PROJECT)"

test:
	$(DOTNET) test "$(TEST_PROJECT)"

clean:
	$(DOTNET) clean "$(PROJECT)"
	$(DOTNET) clean "$(TEST_PROJECT)"

help:
	@echo "Targets:"
	@echo "  build  Build the Option 2 application"
	@echo "  run    Run the SplashKit application"
	@echo "  test   Run the model tests"
	@echo "  clean  Remove generated build output"
	@echo "  help   Show this message"
