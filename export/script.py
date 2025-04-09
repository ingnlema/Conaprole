import json

# Cargar el contenido de los archivos originales
with open("Conaprole-realm.json", "r", encoding="utf-8") as realm_file:
    realm_data = json.load(realm_file)

with open("Conaprole-users-0.json", "r", encoding="utf-8") as users_file:
    users_export = json.load(users_file)
    # Asumiendo que el archivo de usuarios tiene la propiedad "users" con el array de usuarios
    users_array = users_export.get("users", [])

# Fusionar agregando la clave "users" al objeto realm
realm_data["users"] = users_array

# Guardar el archivo fusionado
with open("conaprole-realm-export.json", "w", encoding="utf-8") as output_file:
    json.dump(realm_data, output_file, indent=2)
