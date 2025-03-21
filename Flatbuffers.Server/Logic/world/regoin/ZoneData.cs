﻿namespace Game.Logic.World;

public class ZoneData
{
    public ushort ZoneID
    { get { return m_ZoneID; } set { m_ZoneID = value; } }

    public ushort RegionID
    { get { return m_RegionID; } set { m_RegionID = value; } }

    public byte OffX
    { get { return m_OffX; } set { m_OffX = value; } }

    public byte OffY
    { get { return m_OffY; } set { m_OffY = value; } }

    public byte Height
    { get { return m_Height; } set { m_Height = value; } }

    public byte Width
    { get { return m_Width; } set { m_Width = value; } }

    public string Description
    { get { return m_description; } set { m_description = value; } }

    public byte DivingFlag
    { get { return m_divingFlag; } set { m_divingFlag = value; } }

    public int WaterLevel
    { get { return m_waterLevel; } set { m_waterLevel = value; } }

    public bool IsLava
    { get { return m_IsLava; } set { m_IsLava = value; } }

    private byte m_OffX, m_OffY, m_Height, m_Width;
    private ushort m_ZoneID, m_RegionID;
    private string m_description;
    private int m_waterLevel;
    private byte m_divingFlag;
    private bool m_IsLava;
}