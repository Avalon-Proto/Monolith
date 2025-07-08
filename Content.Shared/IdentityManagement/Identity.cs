// SPDX-FileCopyrightText: 2022 Kara
// SPDX-FileCopyrightText: 2023 metalgearsloth
// SPDX-FileCopyrightText: 2024 Leon Friedrich
// SPDX-FileCopyrightText: 2024 deltanedas
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Whatstone
// SPDX-FileCopyrightText: 2025 ark1368
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Ghost;
using Content.Shared.IdentityManagement.Components;
using Robust.Shared.Player;

namespace Content.Shared.IdentityManagement;

/// <summary>
///     Static content API for getting the identity entities/names for a given entity.
///     This should almost always be used in favor of metadata name, if the entity in question is a human player that
///     can have identity.
/// </summary>
public static class Identity
{
    /// <summary>
    ///     Returns the name that should be used for this entity for identity purposes.
    /// </summary>
    public static string Name(EntityUid uid, IEntityManager ent, EntityUid? viewer=null)
    {
        if (!uid.IsValid() || !ent.TryGetComponent(uid, out MetaDataComponent? meta)) // Frontier: add TryGetComponent
            return string.Empty;

        //var meta = ent.GetComponent<MetaDataComponent>(uid); // Frontier: exception safety
        if (meta.EntityLifeStage <= EntityLifeStage.Initializing)
            return meta.EntityName; // Identity component and such will not yet have initialized and may throw NREs

        // Mono: Ghosts can now see usernames of players
        if (viewer != null && ent.HasComponent<GhostComponent>(viewer.Value) &&
            ent.TryGetComponent<ActorComponent>(uid, out var actorComponent))
        {
            var entityName = meta.EntityName;
            var username = actorComponent.PlayerSession.Name;
            return $"{entityName} ({username})";
        }

        var uidName = meta.EntityName;

        if (!ent.TryGetComponent<IdentityComponent>(uid, out var identity))
            return uidName;

        var ident = identity.IdentityEntitySlot.ContainedEntity;
        if (ident is null || !ent.TryGetComponent(ident.Value, out MetaDataComponent? identMeta)) // Frontier: add TryGetComponent
            return uidName;

        //var identName = ent.GetComponent<MetaDataComponent>(ident.Value).EntityName; // Frontier: exception safety
        var identName = identMeta.EntityName; // Frontier: exception safety
        if (viewer == null || !CanSeeThroughIdentity(uid, viewer.Value, ent))
        {
            return identName;
        }
        if (uidName == identName)
        {
            return uidName;
        }

        return $"{uidName} ({identName})";
    }

    /// <summary>
    ///     Returns the entity that should be used for identity purposes, for example to pass into localization.
    ///     This is an extension method because of its simplicity, and if it was any harder to call it might not
    ///     be used enough for loc.
    /// </summary>
    public static EntityUid Entity(EntityUid uid, IEntityManager ent)
    {
        if (!ent.TryGetComponent<IdentityComponent>(uid, out var identity))
            return uid;

        return identity.IdentityEntitySlot.ContainedEntity ?? uid;
    }

    public static bool CanSeeThroughIdentity(EntityUid uid, EntityUid viewer, IEntityManager ent)
    {
        // Would check for uid == viewer here but I think it's better for you to see yourself
        // how everyone else will see you, otherwise people will probably get confused and think they aren't disguised
        return ent.HasComponent<GhostComponent>(viewer);
    }

}
