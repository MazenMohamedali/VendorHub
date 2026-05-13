// src/data/mockApi.js

export const mockProducts = [
  {
    id: "PROD-001",
    vendorId: "VEND-101",
    vendorName: "تكنو ستور",
    title: "سماعة رأس لاسلكية احترافية",
    description: "سماعة رأس بخاصية عزل الضوضاء، بطارية تدوم 30 ساعة، مناسبة للألعاب والعمل.",
    price: 1200.00,
    category: "إلكترونيات",
    images: [
      "https://placehold.co/600x400/ecfdf5/059669?text=Headphone+1",
      "https://placehold.co/600x400/ecfdf5/059669?text=Headphone+2"
    ],
    availableUnits: 15,
    viewers: 342,
    status: "Approved", 
    ratings: [
      { userId: "CUST-001", rate: 5, comment: "ممتازة جداً وصوتها نقي" }
    ]
  },
  {
    id: "PROD-002",
    vendorId: "VEND-102",
    vendorName: "أناقة شوب",
    title: "حقيبة ظهر للسفر",
    description: "حقيبة ظهر مقاومة للماء مع منفذ USB لشحن الهاتف.",
    price: 450.50,
    category: "أزياء وحقائب",
    images: [
      "https://placehold.co/600x400/ecfdf5/059669?text=Backpack"
    ],
    availableUnits: 0, 
    viewers: 120,
    status: "Approved",
    ratings: []
  }
];

export const mockUsers = [
    { id: "CUST-001", name: "أحمد", role: "Customer" },
    { id: "VEND-101", name: "تكنو ستور", role: "Vendor", isApproved: true },
    { id: "ADMIN-1", name: "مدير النظام", role: "Admin" }
];