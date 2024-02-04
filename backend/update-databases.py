import os
import subprocess

def run_migration_commands(migrations_name, folder_path='Data/DbContexts/'):
    # Ensure the folder exists
    if not os.path.exists(folder_path):
        print(f"The folder {folder_path} does not exist.")
        return

    # List all .cs files in the folder
    files = [f for f in os.listdir(folder_path) if f.endswith('.cs')]
    
    # No files found scenario
    if not files:
        print(f"No .cs files found in {folder_path}.")
        return

    # Run migration add command for each file
    for file in files:
        context_name = file.replace('.cs', '')
        add_command = f'dotnet ef migrations add "{migrations_name}" --context {context_name}'
        print(f"Running: {add_command}")
        subprocess.run(add_command, shell=True)
    
    # Run database update command for each file after adding all migrations
    for file in files:
        context_name = file.replace('.cs', '')
        update_command = f'dotnet ef database update --context {context_name}'
        print(f"Running: {update_command}")
        subprocess.run(update_command, shell=True)

if __name__ == "__main__":
    migrations_name = input("Enter the migrations name: ")
    run_migration_commands(migrations_name)
