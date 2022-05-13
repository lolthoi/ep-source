export const navigation = [
  {
    text: 'Home',
    path: '/home',
    icon: 'home',
  },
  {
    text: 'User',
    path: '/user',
    icon: 'user',
  },
  {
    text: 'Project',
    path: '/project',
    icon: 'activefolder',
  },
  {
    text: 'Evaluation Criteria',
    path: '/criteria',
    icon: 'hierarchy',
  },
  {
    text: 'Evaluation Template',
    path: '/evaluation-template',
    icon: 'verticalaligntop',
  },
  {
    text: 'Evaluation',
    icon: 'background',
    items: [
      {
        text: 'Personal Evaluation',
        path: '/evaluation-personal',
      },
      {
        text: 'Evaluation Management',
        path: '/evaluation-manage',
      },
      {
        text: 'Leader Evaluation',
        path: '/evaluation-leader',
      },
    ],
  },
  {
    text: 'Timesheet',
    icon: 'clock',
    items: [
      {
        text: 'Work Space Setting',
        path: '/timesheet/manage-workspace',
      },
      {
        text: 'Group',
        path: '/timesheet/group',
      },
      {
        text: 'TimeTracker',
        path: '/timesheet/time-tracker'
      }, 
      {
        text: 'Report',
        path: '/timesheet/report'
      }
    ]
  },
];
