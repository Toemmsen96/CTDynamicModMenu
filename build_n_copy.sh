#!/bin/bash

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Function to print colored output
print_colored() {
    echo -e "${1}${2}${NC}"
}

# Function to print a separator
print_separator() {
    echo -e "${CYAN}================================================${NC}"
}

# Auto-detect project name from .csproj file
PROJECT_NAME=""
CSPROJ_FILE=""

# Find .csproj file in current directory
for file in *.csproj; do
    if [ -f "$file" ]; then
        CSPROJ_FILE="$file"
        PROJECT_NAME=$(basename "$file" .csproj)
        break
    fi
done

# If no .csproj found, fall back to directory name
if [ -z "$PROJECT_NAME" ]; then
    PROJECT_NAME=$(basename "$(pwd)")
    print_colored $YELLOW "⚠️  No .csproj file found, using directory name: $PROJECT_NAME"
else
    print_colored $BLUE "🔍 Detected project: $PROJECT_NAME (from $CSPROJ_FILE)"
fi

TARGET_PATH=$(cat path.txt | tr -d '\n\r')
# Expand environment variables like $HOME
TARGET_PATH=$(eval echo "$TARGET_PATH")
print_colored $BLUE "📁 Target path: $TARGET_PATH"

# Check if target directory exists
if [ ! -d "$TARGET_PATH" ]; then
    print_colored $RED "❌ Error: Target directory does not exist: $TARGET_PATH"
    
    # Check if Steam common directory exists
    STEAM_COMMON="$HOME/.steam/steam/steamapps/common"
    if [ -d "$STEAM_COMMON" ]; then
        print_colored $YELLOW "🔍 Searching for available games in Steam..."
        print_colored $CYAN "Available games in $STEAM_COMMON:"
        
        # Create an array to store game directories
        declare -a GAMES=()
        counter=1
        
        # List all directories in Steam common folder
        for dir in "$STEAM_COMMON"/*; do
            if [ -d "$dir" ]; then
                GAME_NAME=$(basename "$dir")
                GAMES+=("$GAME_NAME")
                print_colored $PURPLE "  [$counter] 📂 $GAME_NAME"
                ((counter++))
            fi
        done
        
        if [ ${#GAMES[@]} -eq 0 ]; then
            print_colored $RED "❌ No games found in Steam directory!"
            exit 1
        fi
        
        print_separator
        print_colored $YELLOW "💡 Select a game to update path.txt, or press Enter to exit:"
        read -p "Enter game number (1-${#GAMES[@]}): " selection
        
        # Validate selection
        if [[ "$selection" =~ ^[0-9]+$ ]] && [ "$selection" -ge 1 ] && [ "$selection" -le ${#GAMES[@]} ]; then
            SELECTED_GAME="${GAMES[$((selection-1))]}"
            NEW_PATH="$STEAM_COMMON/$SELECTED_GAME/BepInEx/plugins"
            
            print_colored $GREEN "✅ Selected: $SELECTED_GAME"
            print_colored $BLUE "📝 Updating path.txt with: \$HOME/.steam/steam/steamapps/common/$SELECTED_GAME/BepInEx/plugins"
            
            # Update path.txt
            echo "\$HOME/.steam/steam/steamapps/common/$SELECTED_GAME/BepInEx/plugins" > path.txt
            
            # Create the BepInEx/plugins directory if it doesn't exist
            if [ ! -d "$NEW_PATH" ]; then
                print_colored $YELLOW "🔧 Creating BepInEx/plugins directory..."
                mkdir -p "$NEW_PATH"
                if [ $? -eq 0 ]; then
                    print_colored $GREEN "✅ Directory created successfully!"
                else
                    print_colored $RED "❌ Failed to create directory!"
                    exit 1
                fi
            fi
            
            # Update TARGET_PATH and continue with build
            TARGET_PATH="$NEW_PATH"
            print_colored $GREEN "🔄 Continuing with build process..."
        else
            print_colored $YELLOW "👋 No valid selection made. Exiting..."
            exit 0
        fi
    else
        print_colored $RED "❌ Steam directory not found at: $STEAM_COMMON"
        print_colored $YELLOW "💡 Please verify Steam installation and game location"
        exit 1
    fi
fi

print_separator
print_colored $YELLOW "🔨 Starting build process..."
print_separator

# Build the project
print_colored $CYAN "⚙️  Building $PROJECT_NAME..."
dotnet build --configuration Release

# Check if build was successful
if [ $? -eq 0 ]; then
    print_colored $GREEN "✅ Build completed successfully!"
else
    print_colored $RED "❌ Build failed!"
    exit 1
fi

print_separator
print_colored $YELLOW "📦 Copying DLLs to target location..."

# Find the built DLL using the detected project name
DLL_PATH=$(find . -name "${PROJECT_NAME}.dll" -path "*/bin/Release/*" | head -1)

if [ -z "$DLL_PATH" ]; then
    print_colored $RED "❌ Error: Could not find ${PROJECT_NAME}.dll in build output!"
    print_colored $YELLOW "🔍 Searching for any .dll files in build output..."
    
    # Try to find any DLL in the build output as fallback
    FALLBACK_DLL=$(find . -name "*.dll" -path "*/bin/Release/*" | grep -v "ref/" | head -1)
    if [ -n "$FALLBACK_DLL" ]; then
        print_colored $YELLOW "⚠️  Found fallback DLL: $FALLBACK_DLL"
        DLL_PATH="$FALLBACK_DLL"
    else
        print_colored $RED "❌ No DLL files found in build output!"
        exit 1
    fi
fi

print_colored $BLUE "📄 Found main DLL: $DLL_PATH"

# Extract the actual DLL name from the path
DLL_NAME=$(basename "$DLL_PATH")

# Copy the main DLL to the target location
cp "$DLL_PATH" "$TARGET_PATH/"

if [ $? -eq 0 ]; then
    print_colored $GREEN "✅ Main DLL ($DLL_NAME) copied successfully to $TARGET_PATH"
    
    # Show file info
    COPIED_FILE="$TARGET_PATH/$DLL_NAME"
    if [ -f "$COPIED_FILE" ]; then
        FILE_SIZE=$(du -h "$COPIED_FILE" | cut -f1)
        FILE_DATE=$(date -r "$COPIED_FILE" "+%Y-%m-%d %H:%M:%S")
        print_colored $PURPLE "📊 Main DLL size: $FILE_SIZE"
        print_colored $PURPLE "🕒 Modified: $FILE_DATE"
    fi
else
    print_colored $RED "❌ Error: Failed to copy main DLL!"
    exit 1
fi

# Check if out.txt exists and copy additional DLLs
if [ -f "out.txt" ]; then
    print_colored $YELLOW "📋 Found out.txt, copying additional DLLs..."
    
    while IFS= read -r dll_name; do
        # Skip empty lines
        if [ -z "$dll_name" ]; then
            continue
        fi
        
        # Remove any whitespace/newlines
        dll_name=$(echo "$dll_name" | tr -d '\n\r' | xargs)
        
        # Find the DLL in build output
        ADDITIONAL_DLL_PATH=$(find . -name "$dll_name" -path "*/bin/Release/*" | head -1)
        
        if [ -n "$ADDITIONAL_DLL_PATH" ]; then
            print_colored $BLUE "📄 Found additional DLL: $ADDITIONAL_DLL_PATH"
            cp "$ADDITIONAL_DLL_PATH" "$TARGET_PATH/"
            
            if [ $? -eq 0 ]; then
                print_colored $GREEN "✅ Copied $dll_name successfully"
                
                # Show file info
                COPIED_ADDITIONAL="$TARGET_PATH/$dll_name"
                if [ -f "$COPIED_ADDITIONAL" ]; then
                    FILE_SIZE=$(du -h "$COPIED_ADDITIONAL" | cut -f1)
                    print_colored $PURPLE "📊 $dll_name size: $FILE_SIZE"
                fi
            else
                print_colored $RED "❌ Failed to copy $dll_name"
            fi
        else
            print_colored $YELLOW "⚠️  Could not find $dll_name in build output"
        fi
    done < out.txt
else
    print_colored $BLUE "ℹ️  No out.txt found, skipping additional DLLs"
fi

print_separator
print_colored $GREEN "🎉 Build and copy completed successfully!"
print_colored $CYAN "🚀 Ready to test in $PROJECT_NAME!"
print_separator