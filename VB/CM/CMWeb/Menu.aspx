<%@ Page Title="" Language="VB" MasterPageFile="~/CM.master" AutoEventWireup="true" CodeFile="Menu.aspx.vb" Inherits="Menu" %>
<%@ MasterType  virtualPath="~/CM.master"%>

<asp:Content ID="Content1" ContentPlaceHolderID="Content1" Runat="Server">
    <asp:Menu ID="Menu1" runat="server" onmenuitemclick="Menu1_MenuItemClick" 
        Orientation="Horizontal">
        <Items>
            <asp:MenuItem Text="掲示板" Value="掲示板"></asp:MenuItem>
        </Items>
        <LevelMenuItemStyles>
            <asp:MenuItemStyle BorderStyle="Solid" BorderWidth="1px" Font-Size="12pt" 
                Font-Underline="False" HorizontalPadding="4px" VerticalPadding="4px" />
        </LevelMenuItemStyles>
    </asp:Menu>
    <asp:MultiView ID="MultiView1" runat="server" >
        <asp:View ID="View1" runat="server">
            <asp:GridView ID="GridView1" runat="server" AllowPaging="True" AutoGenerateColumns="False"
                PageSize="20" 
                Width="100%">
                <Columns>
                    <asp:BoundField DataField="記事NO" HeaderText="№"  ><ItemStyle Width="40px" HorizontalAlign="Center" /></asp:BoundField>
                    <asp:BoundField DataField="重要度" HeaderText="重要度"  ><ItemStyle Width="60px" HorizontalAlign="Center" /></asp:BoundField>
                    <asp:BoundField DataField="表示期間From" HeaderText="日付" DataFormatString="{0:yyyy/MM/dd}"><ItemStyle Width="80px" /></asp:BoundField>
                    <asp:BoundField DataField="内容" HeaderText="内容" />
                </Columns>
                <HeaderStyle BackColor="#003580" ForeColor="White" Height="18px" />
                <AlternatingRowStyle BackColor="#CCCCFF" />
                <PagerSettings Mode="NumericFirstLast" />
                <PagerStyle BackColor="Control" />
            </asp:GridView>    
        </asp:View>
        <asp:View ID="View2" runat="server">
            <asp:Menu ID="Menu2" runat="server" CssClass="menu">
                <LevelMenuItemStyles>
                    <asp:MenuItemStyle BorderStyle="Outset" Font-Size="14pt" Font-Underline="False" 
                        VerticalPadding="4px" ItemSpacing="4px" Width="300px" />
                </LevelMenuItemStyles>
            </asp:Menu>
        </asp:View>
    </asp:MultiView>
</asp:Content>

