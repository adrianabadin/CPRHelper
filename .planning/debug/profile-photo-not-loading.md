---
status: awaiting_human_verify
trigger: "profile-photo-not-loading - Al cambiar la foto de perfil, el upload falla con 'The resource already exists' y la imagen nunca se muestra en la UI ni en el avatar."
created: 2026-03-31T00:00:00Z
updated: 2026-03-31T00:02:00Z
---

## Current Focus

hypothesis: CONFIRMADA y resuelta
test: Aplicados dos fixes en AuthService.cs lineas 359 y 364
expecting: Upload con upsert sobrescribe el archivo, URL con timestamp fuerza recarga de imagen
next_action: Esperar confirmación humana de que el flujo funciona en el dispositivo

## Symptoms

expected: Al seleccionar una nueva foto de perfil, debe subirse al storage de Supabase (sobreescribiendo la existente), actualizarse la URL en el perfil del usuario, y mostrarse la nueva imagen en la pantalla de perfil y en el avatar.
actual: El dialogo de selección aparece, pero la imagen nunca se muestra. El upload falla silenciosamente desde la perspectiva del usuario.
errors: |
  [AuthService] UploadAvatarAsync: uploading avatar for user e4f99058-a772-488c-bd62-04a20a9124bf
  Excepción producida: 'Supabase.Storage.Exceptions.SupabaseStorageException' en System.Private.CoreLib.dll
  [AuthService] UploadAvatarAsync failed: The resource already exists
reproduction: 1. Ir a Modificar Perfil, 2. Hacer click en la foto de perfil, 3. Seleccionar una imagen desde el dialogo, 4. La imagen nunca se actualiza
started: Feature nueva, nunca funcionó correctamente
platform: Windows (.NET MAUI)

## Eliminated

- hypothesis: Error en la capa de UI (binding) como causa primaria
  evidence: El binding UserAvatarUrl en ProfilePage.xaml es correcto - usa DataTrigger y está enlazado a la propiedad correcta del ViewModel. AuthViewModel.UploadAvatarAsync ya asigna UserAvatarUrl = avatarUrl después del upload.
  timestamp: 2026-03-31T00:01:00Z

## Evidence

- timestamp: 2026-03-31T00:01:00Z
  checked: AuthService.cs línea 358 (antes del fix)
  found: storage.Upload(fileBytes, fileName) sin opciones — el SDK de Supabase Storage usa Upload por defecto sin upsert
  implication: Cuando el avatar ya existe (segunda vez que el usuario cambia foto), la API de Supabase retorna error 409 "The resource already exists" lanzando SupabaseStorageException

- timestamp: 2026-03-31T00:01:00Z
  checked: AuthService.cs línea 361 (antes del fix) - storage.GetPublicUrl(fileName)
  found: La URL pública no incluye cache-busting. La URL es siempre idéntica (e.g. .../avatars/uuid/avatar.jpg)
  implication: Incluso con upsert funcionando, el Image control de MAUI cachea la imagen por URL y no recargará la imagen nueva porque la URL es la misma string

- timestamp: 2026-03-31T00:01:00Z
  checked: SDK Supabase.Storage 2.0.2 XML docs + método signatures
  found: StorageFileApi.Upload acepta FileOptions como tercer parámetro. El SDK también expone StorageFileApi.Update para reemplazar archivos existentes.
  implication: new FileOptions { Upsert = true } es la opción correcta para hacer upload idempotente

- timestamp: 2026-03-31T00:01:00Z
  checked: AuthViewModel.cs líneas 443-480, ProfilePage.xaml líneas 21-35
  found: El ViewModel asigna UserAvatarUrl = avatarUrl tras upload exitoso. ProfilePage.xaml tiene Image Source="{Binding UserAvatarUrl}" con DataTrigger para null que muestra person_icon.png. El binding es correcto.
  implication: La capa UI es correcta - una vez que UploadAvatarAsync retorne una URL válida (en lugar de null), la imagen se mostrará automáticamente

## Resolution

root_cause: |
  BUG 1 (primario - bloquea todo): AuthService.UploadAvatarAsync (línea 358) llama a storage.Upload()
  sin opciones. Supabase Storage trata Upload como creación exclusiva. Si el archivo ya existe retorna
  409 y el SDK lanza SupabaseStorageException "The resource already exists". El catch de la función
  retorna null, y AuthViewModel interpreta null como fallo mostrando solo "Error al subir foto" sin
  ningún cambio en la UI.

  BUG 2 (secundario - afecta reintentos visuales): La URL pública retornada por GetPublicUrl es
  siempre la misma string. El Image control de MAUI cachea imágenes por URL. Después del primer upload
  exitoso, si el usuario cambia la foto de nuevo (con upsert), la URL no cambia y el control muestra
  la imagen vieja en memoria en lugar de descargar la nueva.

fix: |
  En AuthService.cs método UploadAvatarAsync:
  1. Línea 359: Reemplazado storage.Upload(fileBytes, fileName) por
     storage.Upload(fileBytes, fileName, new Supabase.Storage.FileOptions { Upsert = true })
     — esto hace el upload idempotente (overwrite si existe, create si no existe)
  2. Líneas 363-364: Reemplazado publicUrl = storage.GetPublicUrl(fileName) por
     baseUrl = storage.GetPublicUrl(fileName) y publicUrl = $"{baseUrl}?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
     — el timestamp como query parameter fuerza al Image control a descargar la imagen actualizada

verification: |
  Pendiente verificación humana. La lógica del fix es directamente observable en el código:
  - FileOptions.Upsert = true mapea al header x-upsert: true en la API REST de Supabase Storage
  - El timestamp en la URL garantiza string diferente en cada upload, invalidando el cache del control

files_changed:
  - AclsTracker/Services/Auth/AuthService.cs
