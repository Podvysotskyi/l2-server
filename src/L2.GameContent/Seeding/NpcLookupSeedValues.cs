using L2.GameContent.Identifiers;

namespace L2.GameContent.Seeding;

public static class NpcLookupSeedValues
{
    public static IReadOnlyList<(NpcTypeId Id, string Name)> Types { get; } =
    [
        (NpcTypeId.Adventurer, "Adventurer"),
        (NpcTypeId.Artefact, "Artefact"),
        (NpcTypeId.Auctioneer, "Auctioneer"),
        (NpcTypeId.BabyPet, "BabyPet"),
        (NpcTypeId.BroadcastingTower, "BroadcastingTower"),
        (NpcTypeId.CastleDoorman, "CastleDoorman"),
        (NpcTypeId.Chest, "Chest"),
        (NpcTypeId.ClanHallDoorman, "ClanHallDoorman"),
        (NpcTypeId.ClanHallManager, "ClanHallManager"),
        (NpcTypeId.ControlTower, "ControlTower"),
        (NpcTypeId.DawnPriest, "DawnPriest"),
        (NpcTypeId.Defender, "Defender"),
        (NpcTypeId.Doorman, "Doorman"),
        (NpcTypeId.DungeonGatekeeper, "DungeonGatekeeper"),
        (NpcTypeId.DuskPriest, "DuskPriest"),
        (NpcTypeId.EffectPoint, "EffectPoint"),
        (NpcTypeId.EventMonster, "EventMonster"),
        (NpcTypeId.FeedableBeast, "FeedableBeast"),
        (NpcTypeId.FestivalGuide, "FestivalGuide"),
        (NpcTypeId.FestivalMonster, "FestivalMonster"),
        (NpcTypeId.Fisherman, "Fisherman"),
        (NpcTypeId.FlameTower, "FlameTower"),
        (NpcTypeId.FlyTerrainObject, "FlyTerrainObject"),
        (NpcTypeId.Folk, "Folk"),
        (NpcTypeId.FriendlyMob, "FriendlyMob"),
        (NpcTypeId.GrandBoss, "GrandBoss"),
        (NpcTypeId.Guard, "Guard"),
        (NpcTypeId.Merchant, "Merchant"),
        (NpcTypeId.Monster, "Monster"),
        (NpcTypeId.OlympiadManager, "OlympiadManager"),
        (NpcTypeId.Pet, "Pet"),
        (NpcTypeId.PetManager, "PetManager"),
        (NpcTypeId.RaceManager, "RaceManager"),
        (NpcTypeId.RaidBoss, "RaidBoss"),
        (NpcTypeId.RiftInvader, "RiftInvader"),
        (NpcTypeId.SchemeBuffer, "SchemeBuffer"),
        (NpcTypeId.Servitor, "Servitor"),
        (NpcTypeId.SignsPriest, "SignsPriest"),
        (NpcTypeId.TamedBeast, "TamedBeast"),
        (NpcTypeId.Teleporter, "Teleporter"),
        (NpcTypeId.Trainer, "Trainer"),
        (NpcTypeId.VillageMasterDElf, "VillageMasterDElf"),
        (NpcTypeId.VillageMasterDwarf, "VillageMasterDwarf"),
        (NpcTypeId.VillageMasterFighter, "VillageMasterFighter"),
        (NpcTypeId.VillageMasterMystic, "VillageMasterMystic"),
        (NpcTypeId.VillageMasterOrc, "VillageMasterOrc"),
        (NpcTypeId.VillageMasterPriest, "VillageMasterPriest"),
        (NpcTypeId.Warehouse, "Warehouse")
    ];

    public static IReadOnlyList<(NpcRaceId Id, string Name)> Races { get; } =
    [
        (NpcRaceId.Human, "HUMAN"),
        (NpcRaceId.Elf, "ELF"),
        (NpcRaceId.DarkElf, "DARK_ELF"),
        (NpcRaceId.Orc, "ORC"),
        (NpcRaceId.Dwarf, "DWARF"),
        (NpcRaceId.Animal, "ANIMAL"),
        (NpcRaceId.Beast, "BEAST"),
        (NpcRaceId.Bug, "BUG"),
        (NpcRaceId.CastleGuard, "CASTLE_GUARD"),
        (NpcRaceId.Construct, "CONSTRUCT"),
        (NpcRaceId.Demonic, "DEMONIC"),
        (NpcRaceId.Divine, "DIVINE"),
        (NpcRaceId.Dragon, "DRAGON"),
        (NpcRaceId.Elemental, "ELEMENTAL"),
        (NpcRaceId.Etc, "ETC"),
        (NpcRaceId.Fairy, "FAIRY"),
        (NpcRaceId.Giant, "GIANT"),
        (NpcRaceId.Humanoid, "HUMANOID"),
        (NpcRaceId.Mercenary, "MERCENARY"),
        (NpcRaceId.Plant, "PLANT"),
        (NpcRaceId.SiegeWeapon, "SIEGE_WEAPON"),
        (NpcRaceId.Undead, "UNDEAD")
    ];

    public static IReadOnlyList<(NpcSexId Id, string Name)> Sexes { get; } =
    [
        (NpcSexId.Male, "MALE"),
        (NpcSexId.Female, "FEMALE"),
        (NpcSexId.Etc, "ETC")
    ];
}
