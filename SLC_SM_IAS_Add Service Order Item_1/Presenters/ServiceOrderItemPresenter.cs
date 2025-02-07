namespace SLC_SM_IAS_Add_Service_Order_Item_1.Presenters
{
	using System;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Library;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using SLC_SM_IAS_Add_Service_Order_Item_1.Views;

	public class ServiceOrderItemPresenter
	{
		private readonly IEngine engine;
		private readonly ServiceOrderItemView view;
		private readonly Repo repo;
		private readonly string[] getServiceOrderItemLabels;
		private ServiceOrderItemsInstance instanceToReturn;

		public ServiceOrderItemPresenter(IEngine engine, ServiceOrderItemView view, Repo repo, string[] getServiceOrderItemLabels)
		{
			this.engine = engine;
			this.view = view;
			this.repo = repo;
			this.getServiceOrderItemLabels = getServiceOrderItemLabels;
			instanceToReturn = new ServiceOrderItemsInstance();

			view.TboxName.Changed += (sender, args) => ValidateLabel(args.Value);
		}

		public ServiceOrderItemsInstance GetData
		{
			get
			{
				instanceToReturn.ServiceOrderItemInfo.Name = view.TboxName.Text;
				instanceToReturn.ServiceOrderItemInfo.Action = view.ActionType.Selected.ToString();
				instanceToReturn.ServiceOrderItemServiceInfo.Configuration = view.Configuration.Selected?.ID.Id;
				instanceToReturn.ServiceOrderItemServiceInfo.Properties = view.Properties.Selected?.ID.Id;
				instanceToReturn.ServiceOrderItemServiceInfo.ServiceCategory = view.Category.Selected?.ID.Id;
				instanceToReturn.ServiceOrderItemServiceInfo.ServiceSpecification = view.Specification.Selected?.ID.Id;
				instanceToReturn.ServiceOrderItemServiceInfo.Service = view.Service.Selected?.ID.Id;
				return instanceToReturn;
			}
		}

		public void LoadFromModel()
		{
			// Load correct types
			view.Category.SetOptions(repo.AllCategories.Select(x => new Option<ServiceCategoryInstance>(x.Name, x)));
			view.Properties.SetOptions(repo.AllPropertyValues.Select(x => new Option<ServicePropertyValuesInstance>(x.Name, x)));
			view.Configuration.SetOptions(repo.AllConfigurations.Select(x => new Option<ServiceConfigurationInstance>(x.Name, x)));
			view.Specification.SetOptions(repo.AllSpecs.Select(x => new Option<ServiceSpecificationsInstance>(x.Name, x)));
			view.Service.SetOptions(repo.AllServices.Select(x => new Option<ServicesInstance>(x.Name, x)));
		}

		public void LoadFromModel(ServiceOrderItemsInstance instance)
		{
			instanceToReturn = instance;

			// Load correct types
			LoadFromModel();

			view.BtnAdd.Text = "Edit";
			view.TboxName.Text = instance.Name;
			view.ActionType.Selected = Enum.TryParse(instance.ServiceOrderItemInfo.Action, true, out ServiceOrderItemView.ActionTypeEnum action) ? action : ServiceOrderItemView.ActionTypeEnum.NoChange;

			view.Category.Selected = repo.AllCategories.FirstOrDefault(x => x.ID.Id == instance.ServiceOrderItemServiceInfo.ServiceCategory);
			view.Properties.Selected = repo.AllPropertyValues.FirstOrDefault(x => x.ID.Id == instance.ServiceOrderItemServiceInfo.Properties);
			view.Configuration.Selected = repo.AllConfigurations.FirstOrDefault(x => x.ID.Id == instance.ServiceOrderItemServiceInfo.Configuration);
			view.Specification.Selected = repo.AllSpecs.FirstOrDefault(x => x.ID.Id == instance.ServiceOrderItemServiceInfo.ServiceSpecification);
			view.Service.Selected = repo.AllServices.FirstOrDefault(x => x.ID.Id == instance.ServiceOrderItemServiceInfo.Service);
		}

		public bool Validate()
		{
			bool ok = true;

			ok &= ValidateLabel(view.TboxName.Text);

			return ok;
		}

		private bool ValidateLabel(string newValue)
		{
			if (String.IsNullOrWhiteSpace(newValue))
			{
				view.ErrorName.Text = "Please enter a value!";
				return false;
			}

			if (getServiceOrderItemLabels.Contains(newValue))
			{
				view.ErrorName.Text = "Label already exists!";
				return false;
			}

			view.ErrorName.Text = String.Empty;
			return true;
		}
	}
}