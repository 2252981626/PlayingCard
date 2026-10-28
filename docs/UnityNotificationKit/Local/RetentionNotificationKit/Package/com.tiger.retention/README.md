# Retention Notification

Android local recall notification core. Version 1.0.0.

Requires Unity Mobile Notifications (com.unity.mobile.notifications) 2.4.0.
This package contains scheduling, night protection, click-source reading and icon setup.
Use the standalone kit INTEGRATION.md and its RetentionSetup.cs adapter for game-entry
permission, progress synchronization and foreground/background lifecycle handling.
The adapter disables this core package's separate auto-launch hook to prevent double scheduling.
