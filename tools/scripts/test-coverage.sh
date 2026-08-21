#!/usr/bin/env bash

# Define target project directories relative to tests/
PROJECTS=(
  "SyncUp.Agent.Tests"
)

for PROJECT_NAME in "${PROJECTS[@]}"; do
  echo "========================================"
  echo "Processing: $PROJECT_NAME"
  echo "========================================"

  # Navigate to the target project directory; skip if missing
  pushd "../../tests/${PROJECT_NAME}" > /dev/null || { echo "Project folder not found. Skipping."; continue; }

  # Clean up previous test results and reports
  rm -rf TestResults coveragereport

  # Run tests with coverage
  dotnet test --collect:"XPlat Code Coverage"

  # Generate HTML coverage report
  reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

  # Open the coverage report in the default browser
  if [ -f "coveragereport/index.html" ]; then
    URL="coveragereport/index.html"
    
    # Try Windows (Git Bash), then macOS, then Linux
    start "$URL" 2>/dev/null \
      || open "$URL" 2>/dev/null \
      || xdg-open "$URL" 2>/dev/null
  fi

  # Return to tools/scripts folder
  popd > /dev/null
done