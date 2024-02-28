import os
import subprocess

def run_operations(migrations_name=None, folder_path='Data/DbContexts/'):
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

    # Ask if tables should be deleted
    delete_tables = input("Do you want to delete the database tables? (Y/N): ").lower()
    if delete_tables == 'y':
        delete_tables_confirmation = input("\033[91mAre you ABSOLUTELY SURE AND KNOW WHAT YOU ARE DOING? (Y/N): \033[0m").lower()
        if delete_tables_confirmation == 'y':
            for file in files:
                context_name = file.replace('.cs', '')
                delete_database_tables(context_name)

    # Only ask if migration is needed if a migrations_name has been provided
    if migrations_name:
        perform_migration = input("Do you want to perform a migration? (Y/N): ").lower()
        if perform_migration == 'y':
            # Run migration add command for each file
            for file in files:
                context_name = file.replace('.cs', '')
                add_command = f'dotnet ef migrations add "{migrations_name}" --context {context_name}'
                print(f"Running: {add_command}")
                subprocess.run(add_command, shell=True)

    # Always run database update command for each file
    for file in files:
        context_name = file.replace('.cs', '')
        update_command = f'dotnet ef database update --context {context_name}'
        print(f"Running: {update_command}")
        subprocess.run(update_command, shell=True)

def delete_database_tables(context_name):
    # Example command to delete tables, adjust according to your project setup
    delete_command = f'dotnet ef database drop --context {context_name} --force'
    print(f"Deleting tables for context: {context_name}")
    subprocess.run(delete_command, shell=True)

if __name__ == "__main__":
    migrations_name = None  # Default to None
    # Ask if migration name is provided
    migration_decision = input("Do you want to provide a migration name? (Y/N): ").lower()
    if migration_decision == 'y':
        migrations_name = input("Enter the migrations name: ")
    run_operations(migrations_name)
